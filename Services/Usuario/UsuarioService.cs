using WebSafeDockingAPI.Exceptions;
using WebSafeDockingAPI.Models;
using WebSafeDockingAPI.Models.Common;
using WebSafeDockingAPI.Repositories;
using WebSafeDockingAPI.Utils;

namespace WebSafeDockingAPI.Services
{
    public class UsuarioService
    {
        private readonly IUsuarioRepository _repository;
        private readonly PasswordHasherService _passwordHasher;
        private readonly AuthService _authService;
        private readonly ILogger<UsuarioService> _logger;

        public UsuarioService(
            IUsuarioRepository repository,
            PasswordHasherService passwordHasher,
            AuthService authService,
            ILogger<UsuarioService> logger)
        {
            _repository = repository;
            _passwordHasher = passwordHasher;
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Busca um usuario pelo ID.
        /// </summary>
        public async Task<UsuarioResponseDTO?> GetByIdAsync(int id)
        {
            var usuario = await _repository.GetByIdAsync(id);
            return usuario != null ? UsuarioResponseDTO.FromUsuario(usuario) : null;
        }

        /// <summary>
        /// Cria um novo usuario e envia o link de primeiro acesso por email.
        /// </summary>
        public async Task<UsuarioResponseDTO> CriarUsuarioAsync(UsuarioCreateUpdateDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Cpf))
            {
                throw new ValidationException("O CPF e obrigatorio para criar usuario.");
            }

            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                throw new ValidationException("O e-mail e obrigatorio para envio do primeiro acesso.");
            }

            var novoUsuario = dto.ToEntity();

            // Primeiro acesso: senha sera definida pelo link enviado por e-mail
            novoUsuario.SenhaHash = string.Empty;
            novoUsuario.Ativo = false;

            var usuarioCriado = await _repository.CreateAsync(novoUsuario);

            var emailEnviado = await _authService.EnviarPrimeiroAcessoAsync(usuarioCriado);
            if (!emailEnviado)
            {
                _logger.LogWarning(
                    "Usuario {UsuarioId} criado, mas o e-mail de primeiro acesso nao foi enviado.",
                    usuarioCriado.Id);
            }

            return UsuarioResponseDTO.FromUsuario(usuarioCriado);
        }

        /// <summary>
        /// Atualiza um usuario existente.
        /// </summary>
        public async Task<UsuarioResponseDTO?> AtualizarUsuarioAsync(int id, UsuarioCreateUpdateDTO dto)
        {
            var usuarioExistente = await _repository.GetByIdAsync(id);

            if (usuarioExistente == null) return null;

            usuarioExistente.Nome = dto.Nome;
            usuarioExistente.Cpf = dto.Cpf;
            usuarioExistente.NivelAcesso = dto.NivelAcesso;

            // Se a senha foi informada, gera o hash; senao, mantem a senha atual
            if (!string.IsNullOrEmpty(dto.SenhaHash))
            {
                usuarioExistente.SenhaHash = _passwordHasher.HashPassword(dto.SenhaHash);
            }

            usuarioExistente.Telefone = dto.Telefone;
            usuarioExistente.Email = dto.Email;
            usuarioExistente.Ativo = dto.Ativo;

            var atualizado = await _repository.UpdateAsync(usuarioExistente);

            if (!atualizado) return null;

            return UsuarioResponseDTO.FromUsuario(usuarioExistente);
        }

        /// <summary>
        /// Deleta um usuario pelo ID.
        /// </summary>
        public async Task<bool> DeletarUsuarioAsync(int id) =>
            await _repository.DeleteAsync(id);

        /// <summary>
        /// Busca usuarios com paginacao estilo Supabase.
        /// </summary>
        public async Task<PaginatedResponse<UsuarioResponseDTO>> SearchUsuariosAsync(
            PaginationRequest request)
        {
            var (items, totalCount) = await _repository.SearchAsync(
                request.Page,
                request.PageSize,
                request.Search,
                request.SortBy,
                request.SortOrder
            );

            return PaginationHelper.CreateResponse(items, totalCount, request);
        }
    }
}
