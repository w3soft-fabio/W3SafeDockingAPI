using WebSafeDockingAPI.Models;
using WebSafeDockingAPI.Models.Common;
using WebSafeDockingAPI.Repositories;
using WebSafeDockingAPI.Utils;

namespace WebSafeDockingAPI.Services
{
    public class UsuarioService
    {
        private readonly IUsuarioRepository _repository;

        public UsuarioService(IUsuarioRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Busca um usuário pelo ID.
        /// </summary>
        public async Task<UsuarioResponseDTO?> GetByIdAsync(int id)
        {
            var usuario = await _repository.GetByIdAsync(id);
            return usuario != null ? UsuarioResponseDTO.FromUsuario(usuario) : null;
        }

        /// <summary>
        /// Cria um novo usuário.
        /// </summary>
        public async Task<UsuarioResponseDTO> CriarUsuarioAsync(UsuarioCreateUpdateDTO dto)
        {
            var novoUsuario = dto.ToEntity();

            var usuarioCriado = await _repository.CreateAsync(novoUsuario);

            return UsuarioResponseDTO.FromUsuario(usuarioCriado);
        }

        /// <summary>
        /// Atualiza um usuário existente.
        /// </summary>
        public async Task<UsuarioResponseDTO?> AtualizarUsuarioAsync(int id, UsuarioCreateUpdateDTO dto)
        {
            var usuarioExistente = await _repository.GetByIdAsync(id);

            if (usuarioExistente == null) return null;

            usuarioExistente.Nome = dto.Nome;
            usuarioExistente.Cpf = dto.Cpf;
            usuarioExistente.NivelAcesso = dto.NivelAcesso;
            usuarioExistente.SenhaHash = dto.SenhaHash;
            usuarioExistente.Telefone = dto.Telefone;
            usuarioExistente.Email = dto.Email;
            usuarioExistente.RazaoSocial = dto.RazaoSocial;
            usuarioExistente.Ativo = dto.Ativo;

            var atualizado = await _repository.UpdateAsync(usuarioExistente);

            if (!atualizado) return null;

            return UsuarioResponseDTO.FromUsuario(usuarioExistente);
        }

        /// <summary>
        /// Deleta um usuário pelo ID.
        /// </summary>
        public async Task<bool> DeletarUsuarioAsync(int id) =>
            await _repository.DeleteAsync(id);

        /// <summary>
        /// Busca usuários com paginação estilo Supabase.
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
