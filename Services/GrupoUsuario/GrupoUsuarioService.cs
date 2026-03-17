using WebSafeDockingAPI.Exceptions;
using WebSafeDockingAPI.Models;
using WebSafeDockingAPI.Models.Common;
using WebSafeDockingAPI.Repositories;
using WebSafeDockingAPI.Utils;

namespace WebSafeDockingAPI.Services
{
    public class GrupoUsuarioService
    {
        private readonly IGrupoUsuarioRepository _repository;
        private readonly IGrupoRepository _grupoRepository;
        private readonly IUsuarioRepository _usuarioRepository;

        public GrupoUsuarioService(
            IGrupoUsuarioRepository repository,
            IGrupoRepository grupoRepository,
            IUsuarioRepository usuarioRepository)
        {
            _repository = repository;
            _grupoRepository = grupoRepository;
            _usuarioRepository = usuarioRepository;
        }

        /// <summary>
        /// Busca um vinculo grupo-usuario pela chave composta.
        /// </summary>
        public async Task<GrupoUsuarioResponseDTO?> GetByIdAsync(int grupoId, int usuarioId)
        {
            var entidade = await _repository.GetByIdAsync(grupoId, usuarioId);
            return entidade != null ? GrupoUsuarioResponseDTO.FromGrupoUsuario(entidade) : null;
        }

        /// <summary>
        /// Cria um novo vinculo grupo-usuario.
        /// </summary>
        public async Task<GrupoUsuarioResponseDTO> CriarGrupoUsuarioAsync(GrupoUsuarioCreateUpdateDTO dto)
        {
            await ValidarDependenciasAsync(dto.GrupoId, dto.UsuarioId);

            var existente = await _repository.GetByIdAsync(dto.GrupoId, dto.UsuarioId);
            if (existente != null)
            {
                throw new ValidationException(
                    $"Ja existe vinculo para o grupo {dto.GrupoId} com o usuario {dto.UsuarioId}.");
            }

            var novaEntidade = dto.ToEntity();
            var entidadeCriada = await _repository.CreateAsync(novaEntidade);

            return GrupoUsuarioResponseDTO.FromGrupoUsuario(entidadeCriada);
        }

        /// <summary>
        /// Atualiza um vinculo grupo-usuario existente.
        /// </summary>
        public async Task<GrupoUsuarioResponseDTO?> AtualizarGrupoUsuarioAsync(
            int grupoIdOriginal,
            int usuarioIdOriginal,
            GrupoUsuarioCreateUpdateDTO dto)
        {
            var entidadeExistente = await _repository.GetByIdAsync(grupoIdOriginal, usuarioIdOriginal);
            if (entidadeExistente == null) return null;

            await ValidarDependenciasAsync(dto.GrupoId, dto.UsuarioId);

            var chaveFoiAlterada = grupoIdOriginal != dto.GrupoId || usuarioIdOriginal != dto.UsuarioId;
            if (chaveFoiAlterada)
            {
                var conflito = await _repository.GetByIdAsync(dto.GrupoId, dto.UsuarioId);
                if (conflito != null)
                {
                    throw new ValidationException(
                        $"Ja existe vinculo para o grupo {dto.GrupoId} com o usuario {dto.UsuarioId}.");
                }
            }

            if (chaveFoiAlterada)
            {
                await _repository.DeleteAsync(grupoIdOriginal, usuarioIdOriginal);

                var novaEntidade = dto.ToEntity();
                var recriada = await _repository.CreateAsync(novaEntidade);

                return GrupoUsuarioResponseDTO.FromGrupoUsuario(recriada);
            }

            entidadeExistente.GrupoId = dto.GrupoId;
            entidadeExistente.UsuarioId = dto.UsuarioId;

            var atualizado = await _repository.UpdateAsync(entidadeExistente);
            if (!atualizado) return null;

            return GrupoUsuarioResponseDTO.FromGrupoUsuario(entidadeExistente);
        }

        /// <summary>
        /// Deleta um vinculo grupo-usuario pela chave composta.
        /// </summary>
        public async Task<bool> DeletarGrupoUsuarioAsync(int grupoId, int usuarioId)
        {
            return await _repository.DeleteAsync(grupoId, usuarioId);
        }

        /// <summary>
        /// Busca vinculos grupo-usuario com paginacao estilo Supabase.
        /// </summary>
        public async Task<PaginatedResponse<GrupoUsuarioResponseDTO>> SearchGruposUsuariosAsync(
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

        private async Task ValidarDependenciasAsync(int grupoId, int usuarioId)
        {
            var grupo = await _grupoRepository.GetByIdAsync(grupoId);
            if (grupo == null)
            {
                throw new ValidationException($"Grupo com ID {grupoId} nao encontrado.");
            }

            var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
            if (usuario == null)
            {
                throw new ValidationException($"Usuario com ID {usuarioId} nao encontrado.");
            }
        }
    }
}
