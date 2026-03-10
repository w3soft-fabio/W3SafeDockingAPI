using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using WebSafeDockingAPI.Models;
using WebSafeDockingAPI.Repositories;

namespace WebSafeDockingAPI.Services
{
    /// <summary>
    /// Serviço principal de autenticação.
    /// Responsável por: login, geração de tokens JWT, refresh e revogação.
    /// </summary>
    public class AuthService
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly PasswordHasherService _passwordHasher;
        private readonly IConfiguration _configuration;

        public AuthService(
            IRefreshTokenRepository refreshTokenRepository,
            PasswordHasherService passwordHasher,
            IConfiguration configuration)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }

        // =====================================================
        //  LOGIN - Autentica o usuário e retorna os tokens
        // =====================================================

        /// <summary>
        /// Realiza o login do usuário.
        /// 1. Busca o usuário pelo CPF
        /// 2. Verifica a senha usando BCrypt
        /// 3. Gera o Access Token (JWT) e o Refresh Token
        /// </summary>
        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            // Busca o usuário pelo CPF no banco de dados
            var usuario = await _refreshTokenRepository.BuscarUsuarioPorCpfAsync(request.Cpf);

            // Se o usuário não existe, retorna null (credenciais inválidas)
            if (usuario == null)
                return null;

            // Verifica se o usuário está ativo
            if (usuario.Ativo != true)
                return null;

            // Verifica se a senha está correta comparando com o hash
            if (string.IsNullOrEmpty(usuario.SenhaHash) ||
                !_passwordHasher.VerificarSenha(request.Senha, usuario.SenhaHash))
                return null;

            // Credenciais válidas! Gera os tokens.
            return await GerarTokensAsync(usuario);
        }

        // =====================================================
        //  REFRESH - Renova o Access Token usando o Refresh Token
        // =====================================================

        /// <summary>
        /// Renova o Access Token usando um Refresh Token válido.
        /// O Refresh Token usado é revogado e um novo é gerado (rotação de tokens).
        /// </summary>
        public async Task<LoginResponse?> RefreshAsync(string refreshToken)
        {
            // Busca o refresh token no banco de dados
            var tokenArmazenado = await _refreshTokenRepository.BuscarPorTokenAsync(refreshToken);

            // Verifica se o token existe e está ativo (não revogado e não expirado)
            if (tokenArmazenado == null || !tokenArmazenado.Ativo)
                return null;

            // Busca o usuário dono do token
            var usuario = tokenArmazenado.Usuario
                ?? await _refreshTokenRepository.BuscarUsuarioPorIdAsync(tokenArmazenado.UsuarioId);

            if (usuario == null || usuario.Ativo != true)
                return null;

            // IMPORTANTE: Revoga o refresh token usado (rotação de tokens)
            // Isso garante que cada refresh token só pode ser usado UMA vez
            tokenArmazenado.Revogado = true;
            await _refreshTokenRepository.AtualizarAsync(tokenArmazenado);

            // Gera novos tokens (Access Token + Refresh Token)
            return await GerarTokensAsync(usuario);
        }

        // =====================================================
        //  REVOKE (LOGOUT) - Invalida o Refresh Token
        // =====================================================

        /// <summary>
        /// Revoga (invalida) um Refresh Token.
        /// Usado quando o usuário faz logout.
        /// </summary>
        public async Task<bool> RevogarTokenAsync(string refreshToken)
        {
            // Busca o refresh token no banco
            var tokenArmazenado = await _refreshTokenRepository.BuscarPorTokenAsync(refreshToken);

            // Se não encontrou ou já está revogado, retorna false
            if (tokenArmazenado == null || tokenArmazenado.Revogado)
                return false;

            // Marca como revogado
            tokenArmazenado.Revogado = true;
            await _refreshTokenRepository.AtualizarAsync(tokenArmazenado);

            return true;
        }

        // =====================================================
        //  MÉTODOS PRIVADOS - Geração de tokens
        // =====================================================

        /// <summary>
        /// Gera o Access Token (JWT) e o Refresh Token para um usuário.
        /// </summary>
        private async Task<LoginResponse> GerarTokensAsync(Usuario usuario)
        {
            // 1. Gerar o Access Token (JWT)
            var (accessToken, expiracao) = GerarAccessToken(usuario);

            // 2. Gerar o Refresh Token e salvar no banco
            var refreshToken = await GerarRefreshTokenAsync(usuario.Id);

            // 3. Montar a resposta
            return new LoginResponse
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                Expiracao = expiracao,
                Nome = usuario.Nome ?? "",
                NivelAcesso = usuario.NivelAcesso ?? ""
            };
        }

        /// <summary>
        /// Gera um Access Token JWT contendo as informações (claims) do usuário.
        /// O token é assinado com uma chave secreta e tem validade de 1 hora.
        /// </summary>
        private (string token, DateTime expiracao) GerarAccessToken(Usuario usuario)
        {
            // Lê a chave secreta do appsettings.json
            var jwtKey = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("Chave JWT não configurada no appsettings.json");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Claims = informações do usuário que ficam DENTRO do token
            var claims = new List<Claim>
            {
                // ID do usuário
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),

                // Nome do usuário
                new Claim(ClaimTypes.Name, usuario.Nome ?? ""),

                // CPF do usuário
                new Claim("cpf", usuario.Cpf ?? ""),

                // Nível de acesso (funciona como "role" para autorização)
                new Claim(ClaimTypes.Role, usuario.NivelAcesso ?? ""),

                // Identificador da aplicação
                new Claim("app", "W3SafeDockingAPI")
            };

            // Define quando o token expira (1 hora a partir de agora)
            var expiracao = DateTime.UtcNow.AddHours(1);

            // Cria o token JWT
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiracao,
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return (tokenHandler.WriteToken(token), expiracao);
        }

        /// <summary>
        /// Gera um Refresh Token aleatório e salva no banco de dados.
        /// O refresh token é uma string aleatória (não é um JWT).
        /// Tem validade de 7 dias.
        /// </summary>
        private async Task<string> GerarRefreshTokenAsync(int usuarioId)
        {
            // Gera uma string aleatória segura de 64 bytes, convertida para Base64
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            var tokenString = Convert.ToBase64String(randomBytes);

            // Cria a entidade do refresh token
            var refreshToken = new RefreshToken
            {
                UsuarioId = usuarioId,
                Token = tokenString,
                CriadoEm = DateTime.UtcNow,
                ExpiraEm = DateTime.UtcNow.AddDays(7), // Válido por 7 dias
                Revogado = false
            };

            // Salva no banco de dados
            await _refreshTokenRepository.CriarAsync(refreshToken);

            return tokenString;
        }
    }
}
