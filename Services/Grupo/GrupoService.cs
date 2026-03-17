using WebSafeDockingAPI.Exceptions;
using WebSafeDockingAPI.Models;
using WebSafeDockingAPI.Models.Common;
using WebSafeDockingAPI.Repositories;
using WebSafeDockingAPI.Utils;

namespace WebSafeDockingAPI.Services
{
    public class GrupoService
    {
        private readonly IGrupoRepository _repository;
        private readonly IUsuarioRepository _usuarioRepository;

        public GrupoService(IGrupoRepository repository, IUsuarioRepository usuarioRepository)
        {
            _repository = repository;
            _usuarioRepository = usuarioRepository;
        }

        /// <summary>
        /// Busca um grupo pelo ID.
        /// </summary>
        public async Task<GrupoResponseDTO?> GetByIdAsync(int id)
        {
            var grupo = await _repository.GetByIdAsync(id);
            return grupo != null ? GrupoResponseDTO.FromGrupo(grupo) : null;
        }

        /// <summary>
        /// Cria um novo grupo.
        /// </summary>
        public async Task<GrupoResponseDTO> CriarGrupoAsync(GrupoCreateUpdateDTO dto)
        {
            await ValidarUsuarioAsync(dto.CriadoPorUsuarioId);

            var novoGrupo = dto.ToEntity();
            var grupoCriado = await _repository.CreateAsync(novoGrupo);

            return GrupoResponseDTO.FromGrupo(grupoCriado);
        }

        /// <summary>
        /// Atualiza um grupo existente.
        /// </summary>
        public async Task<GrupoResponseDTO?> AtualizarGrupoAsync(int id, GrupoCreateUpdateDTO dto)
        {
            var grupoExistente = await _repository.GetByIdAsync(id);
            if (grupoExistente == null) return null;

            await ValidarUsuarioAsync(dto.CriadoPorUsuarioId);

            grupoExistente.GrupoNome = dto.GrupoNome;
            grupoExistente.GrupoDescricao = dto.GrupoDescricao;
            grupoExistente.CriadoPorUsuarioId = dto.CriadoPorUsuarioId;

            var atualizado = await _repository.UpdateAsync(grupoExistente);
            if (!atualizado) return null;

            return GrupoResponseDTO.FromGrupo(grupoExistente);
        }

        /// <summary>
        /// Deleta um grupo pelo ID.
        /// </summary>
        public async Task<bool> DeletarGrupoAsync(int id) =>
            await _repository.DeleteAsync(id);

        /// <summary>
        /// Busca grupos por termo com paginacao estilo Supabase.
        /// </summary>
        public async Task<PaginatedResponse<GrupoResponseDTO>> SearchGruposAsync(
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

        private async Task ValidarUsuarioAsync(int usuarioId)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
            if (usuario == null)
            {
                throw new ValidationException($"Usuario com ID {usuarioId} nao encontrado.");
            }
        }
    }
}
