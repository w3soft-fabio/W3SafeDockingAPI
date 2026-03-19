using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using WebSafeDockingAPI.Models;
using WebSafeDockingAPI.Repositories;

namespace WebSafeDockingAPI.Services
{
    /// <summary>
    /// Servico principal de autenticacao.
    /// Responsavel por:
    /// - login (CPF + senha)
    /// - emissao e rotacao de access/refresh tokens
    /// - fluxo de primeiro acesso (link por e-mail + definicao de senha)
    /// </summary>
    public class AuthService
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IPrimeiroAcessoTokenRepository _primeiroAcessoTokenRepository;
        private readonly IEmailContaRepository _emailContaRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly PasswordHasherService _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IRefreshTokenRepository refreshTokenRepository,
            IPrimeiroAcessoTokenRepository primeiroAcessoTokenRepository,
            IEmailContaRepository emailContaRepository,
            IUsuarioRepository usuarioRepository,
            PasswordHasherService passwordHasher,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _primeiroAcessoTokenRepository = primeiroAcessoTokenRepository;
            _emailContaRepository = emailContaRepository;
            _usuarioRepository = usuarioRepository;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
            _logger = logger;
        }

        // =====================================================
        //  LOGIN - Autentica o usuario com CPF e senha
        // =====================================================

        /// <summary>
        /// Realiza o login do usuario.
        /// </summary>
        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            // 1) Busca usuario pelo CPF
            var usuario = await _refreshTokenRepository.BuscarUsuarioPorCpfAsync(request.Cpf);

            // 2) Falha se usuario nao existe
            if (usuario == null)
                return null;

            // 3) Falha se usuario nao esta ativo
            if (usuario.Ativo != true)
                return null;

            // 4) Falha se senha nao confere com o hash salvo
            if (string.IsNullOrEmpty(usuario.SenhaHash) ||
                !_passwordHasher.VerificarSenha(request.Senha, usuario.SenhaHash))
                return null;

            // 5) Credenciais validas -> gera access token + refresh token
            return await GerarTokensAsync(usuario);
        }

        // =====================================================
        //  REFRESH - Renova access token com refresh token
        // =====================================================

        /// <summary>
        /// Renova o access token usando um refresh token valido.
        /// </summary>
        public async Task<LoginResponse?> RefreshAsync(string refreshToken)
        {
            // 1) Busca refresh token armazenado
            var tokenArmazenado = await _refreshTokenRepository.BuscarPorTokenAsync(refreshToken);

            // 2) Token inexistente ou invalido (revogado/expirado)
            if (tokenArmazenado == null || !tokenArmazenado.Ativo)
                return null;

            // 3) Recupera usuario relacionado
            var usuario = tokenArmazenado.Usuario
                ?? await _refreshTokenRepository.BuscarUsuarioPorIdAsync(tokenArmazenado.UsuarioId);

            // 4) Usuario invalido/inativo
            if (usuario == null || usuario.Ativo != true)
                return null;

            // 5) Rotacao de token: revoga o refresh atual
            tokenArmazenado.Revogado = true;
            await _refreshTokenRepository.AtualizarAsync(tokenArmazenado);

            // 6) Emite novo par de tokens
            return await GerarTokensAsync(usuario);
        }

        // =====================================================
        //  REVOKE - Logout (revoga refresh token)
        // =====================================================

        /// <summary>
        /// Revoga um refresh token (logout).
        /// </summary>
        public async Task<bool> RevogarTokenAsync(string refreshToken)
        {
            var tokenArmazenado = await _refreshTokenRepository.BuscarPorTokenAsync(refreshToken);

            if (tokenArmazenado == null || tokenArmazenado.Revogado)
                return false;

            tokenArmazenado.Revogado = true;
            await _refreshTokenRepository.AtualizarAsync(tokenArmazenado);

            return true;
        }

        // =====================================================
        //  PRIMEIRO ACESSO - Envio de link por e-mail
        // =====================================================

        /// <summary>
        /// Envia email de primeiro acesso com link unico para definir senha.
        /// </summary>
        public async Task<bool> EnviarPrimeiroAcessoAsync(Usuario usuario)
        {
            // Validacoes minimas para envio
            if (string.IsNullOrWhiteSpace(usuario.Email) || string.IsNullOrWhiteSpace(usuario.Cpf))
            {
                _logger.LogWarning("Primeiro acesso nao enviado para usuario {UsuarioId}: email/cpf ausente.", usuario.Id);
                return false;
            }

            // Seleciona a conta SMTP padrao (menor ContaID)
            var contaEmail = await _emailContaRepository.GetDefaultAsync();
            if (contaEmail == null)
            {
                _logger.LogWarning("Primeiro acesso nao enviado para usuario {UsuarioId}: nenhuma conta SMTP cadastrada.", usuario.Id);
                return false;
            }

            // Endereco "From": prioriza endereco configurado, senao usuario SMTP
            var fromAddress = !string.IsNullOrWhiteSpace(contaEmail.Endereco)
                ? contaEmail.Endereco!
                : contaEmail.CredenciaisUsuario;

            // Nao tenta enviar com configuracao incompleta
            if (string.IsNullOrWhiteSpace(fromAddress) || string.IsNullOrWhiteSpace(contaEmail.SmtpHost))
            {
                _logger.LogWarning("Primeiro acesso nao enviado para usuario {UsuarioId}: configuracao SMTP incompleta.", usuario.Id);
                return false;
            }

            // Regra: manter somente um token ativo por usuario
            await _primeiroAcessoTokenRepository.InvalidarTokensAtivosAsync(usuario.Id);

            // Gera token em texto (link) e hash (persistencia segura)
            var (token, tokenHash) = GerarTokenPrimeiroAcesso();
            var expiraEm = DateTime.UtcNow.AddMinutes(GetPrimeiroAcessoExpiracaoMinutos());

            // Salva apenas o hash do token no banco
            await _primeiroAcessoTokenRepository.CriarAsync(new PrimeiroAcessoToken
            {
                UsuarioId = usuario.Id,
                TokenHash = tokenHash,
                CriadoEm = DateTime.UtcNow,
                ExpiraEm = expiraEm,
                Revogado = false
            });

            // Monta link e corpo do e-mail
            var link = BuildPrimeiroAcessoLink(token);
            var assunto = "Conta criada com sucesso - Defina sua senha";
            var corpoHtml = BuildPrimeiroAcessoEmailBody(usuario, link, expiraEm);

            // Envia e-mail para o usuario
            return await EnviarEmailAsync(
                contaEmail,
                fromAddress,
                usuario.Email!,
                assunto,
                corpoHtml);
        }

        /// <summary>
        /// Reenvia um novo link de primeiro acesso para o CPF informado.
        /// </summary>
        public async Task<bool> ReenviarPrimeiroAcessoAsync(string cpf)
        {
            // Busca usuario pelo CPF e dispara o mesmo fluxo de envio
            var usuario = await _refreshTokenRepository.BuscarUsuarioPorCpfAsync(cpf);
            if (usuario == null) return false;

            return await EnviarPrimeiroAcessoAsync(usuario);
        }

        // =====================================================
        //  PRIMEIRO ACESSO - Definicao da senha inicial
        // =====================================================

        /// <summary>
        /// Define a primeira senha com base no token de primeiro acesso.
        /// </summary>
        public async Task<bool> DefinirPrimeiraSenhaAsync(DefinirPrimeiraSenhaRequest request)
        {
            // 1) Compara por hash (token nunca e salvo em texto puro)
            var tokenHash = HashToken(request.Token);
            var token = await _primeiroAcessoTokenRepository.BuscarPorHashAsync(tokenHash);

            // 2) Valida token (existe, nao expirou, nao foi usado/revogado)
            if (token == null || !token.Ativo)
                return false;

            // 3) Recupera usuario dono do token
            var usuario = token.Usuario
                ?? await _refreshTokenRepository.BuscarUsuarioPorIdAsync(token.UsuarioId);

            if (usuario == null)
                return false;

            // 4) Grava hash da nova senha e ativa usuario
            usuario.SenhaHash = _passwordHasher.HashPassword(request.NovaSenha);
            usuario.Ativo = true;

            var atualizado = await _usuarioRepository.UpdateAsync(usuario);
            if (!atualizado)
                return false;

            // 5) Consome token (uso unico)
            token.UsadoEm = DateTime.UtcNow;
            token.Revogado = true;
            await _primeiroAcessoTokenRepository.MarcarComoUsadoAsync(token);

            return true;
        }

        // =====================================================
        //  HELPERS DE JWT/REFRESH
        // =====================================================

        /// <summary>
        /// Gera access token (JWT) e refresh token para o usuario.
        /// </summary>
        private async Task<LoginResponse> GerarTokensAsync(Usuario usuario)
        {
            var (accessToken, expiracao) = GerarAccessToken(usuario);
            var refreshToken = await GerarRefreshTokenAsync(usuario.Id);

            return new LoginResponse
            {
                UsuarioId = usuario.Id,
                Cpf = usuario.Cpf ?? "",
                Token = accessToken,
                RefreshToken = refreshToken,
                Expiracao = expiracao,
                Nome = usuario.Nome ?? "",
                NivelAcesso = usuario.NivelAcesso ?? ""
            };
        }

        /// <summary>
        /// Gera o JWT de acesso com claims do usuario.
        /// </summary>
        private (string token, DateTime expiracao) GerarAccessToken(Usuario usuario)
        {
            // Chave de assinatura JWT
            var jwtKey = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("Chave JWT nao configurada no appsettings.json");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Claims principais do usuario para autorizacao
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nome ?? ""),
                new Claim("cpf", usuario.Cpf ?? ""),
                new Claim(ClaimTypes.Role, usuario.NivelAcesso ?? ""),
                new Claim("app", "W3SafeDockingAPI")
            };

            // Token de curta duracao (1h)
            var expiracao = DateTime.UtcNow.AddHours(1);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiracao,
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = credentials
            };

            // Emissao final do token
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return (tokenHandler.WriteToken(token), expiracao);
        }

        /// <summary>
        /// Gera refresh token aleatorio e persiste no banco.
        /// </summary>
        private async Task<string> GerarRefreshTokenAsync(int usuarioId)
        {
            // String aleatoria segura
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            var tokenString = Convert.ToBase64String(randomBytes);

            // Persistencia do refresh token
            var refreshToken = new RefreshToken
            {
                UsuarioId = usuarioId,
                Token = tokenString,
                CriadoEm = DateTime.UtcNow,
                ExpiraEm = DateTime.UtcNow.AddDays(7),
                Revogado = false
            };

            await _refreshTokenRepository.CriarAsync(refreshToken);

            return tokenString;
        }

        // =====================================================
        //  HELPERS DE PRIMEIRO ACESSO
        // =====================================================

        /// <summary>
        /// Gera token de primeiro acesso e seu hash correspondente.
        /// </summary>
        private static (string token, string tokenHash) GerarTokenPrimeiroAcesso()
        {
            var randomBytes = new byte[48];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            var token = Convert.ToBase64String(randomBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", string.Empty);

            return (token, HashToken(token));
        }

        /// <summary>
        /// Aplica SHA-256 ao token (armazenamento seguro no banco).
        /// </summary>
        private static string HashToken(string token)
        {
            var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(hashBytes);
        }

        /// <summary>
        /// Retorna o tempo de expiracao do token de primeiro acesso (minutos).
        /// </summary>
        private int GetPrimeiroAcessoExpiracaoMinutos()
        {
            var configValue = _configuration["FirstAccess:TokenExpirationMinutes"];
            return int.TryParse(configValue, out var minutos) && minutos > 0 ? minutos : 60;
        }

        /// <summary>
        /// Monta o link de primeiro acesso enviado por e-mail.
        /// </summary>
        private string BuildPrimeiroAcessoLink(string token)
        {
            var baseUrl = _configuration["FirstAccess:BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                baseUrl = "https://www.w3soft3.com.br/w3SafeDocking/#";
            }

            var path = _configuration["FirstAccess:Path"];
            if (string.IsNullOrWhiteSpace(path))
            {
                path = "/primeiro-acesso";
            }

            var normalizedPath = path.StartsWith('/') ? path : "/" + path;
            return $"{baseUrl.TrimEnd('/')}{normalizedPath}?token={Uri.EscapeDataString(token)}";
        }

        /// <summary>
        /// Gera o corpo HTML do e-mail de primeiro acesso.
        /// </summary>
        private static string BuildPrimeiroAcessoEmailBody(Usuario usuario, string link, DateTime expiraEmUtc)
        {
            var cpf = MascararCpf(usuario.Cpf);
            var expiracaoTexto = expiraEmUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm");

            return $@"
                <h2>Conta criada com sucesso</h2>
                <p>Ola, <strong>{WebUtility.HtmlEncode(usuario.Nome ?? "usuario")}</strong>.</p>
                <p>Seu cadastro foi concluido.</p>
                <p><strong>CPF:</strong> {WebUtility.HtmlEncode(cpf)}</p>
                <p>Para definir sua senha de acesso, use o link abaixo:</p>
                <p><a href='{WebUtility.HtmlEncode(link)}'>Definir senha de primeiro acesso</a></p>
                <p>Este link expira em: <strong>{WebUtility.HtmlEncode(expiracaoTexto)}</strong>.</p>
                <p>Se voce nao solicitou este acesso, ignore este e-mail.</p>";
        }

        /// <summary>
        /// Mascara CPF para exibir no e-mail sem expor todos os digitos.
        /// </summary>
        private static string MascararCpf(string? cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf)) return "";

            var digits = new string(cpf.Where(char.IsDigit).ToArray());
            if (digits.Length != 11) return cpf;

            return $"***.***.{digits.Substring(6, 3)}-{digits.Substring(9, 2)}";
        }

        // =====================================================
        //  HELPER SMTP
        // =====================================================

        /// <summary>
        /// Envia e-mail HTML via SMTP usando configuracao de EmailConta.
        /// </summary>
        private async Task<bool> EnviarEmailAsync(
            EmailConta conta,
            string fromAddress,
            string toAddress,
            string assunto,
            string corpoHtml)
        {
            try
            {
                // Montagem da mensagem
                using var message = new MailMessage();
                message.From = new MailAddress(fromAddress, conta.Apelido ?? "W3 SafeDocking");
                message.To.Add(toAddress);
                message.Subject = assunto;
                message.Body = corpoHtml;
                message.IsBodyHtml = true;

                // Porta padrao de fallback
                var porta = int.TryParse(conta.SmtpPort, out var parsedPort) && parsedPort > 0
                    ? parsedPort
                    : 587;

                // Cliente SMTP
                using var smtpClient = new SmtpClient(conta.SmtpHost, porta)
                {
                    EnableSsl = string.Equals(conta.SmtpSSL, "True", StringComparison.OrdinalIgnoreCase),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false
                };

                // Credenciais SMTP
                if (!string.IsNullOrWhiteSpace(conta.CredenciaisUsuario) &&
                    !string.IsNullOrWhiteSpace(conta.CredenciaisSenha))
                {
                    smtpClient.Credentials = new NetworkCredential(
                        conta.CredenciaisUsuario,
                        conta.CredenciaisSenha);
                }

                // Disparo do e-mail
                await smtpClient.SendMailAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao enviar email de primeiro acesso para {Destinatario}", toAddress);
                return false;
            }
        }
    }
}
