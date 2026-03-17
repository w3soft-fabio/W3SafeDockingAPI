using WebSafeDockingAPI.Exceptions;
using WebSafeDockingAPI.Models;
using WebSafeDockingAPI.Models.Common;
using WebSafeDockingAPI.Repositories;
using WebSafeDockingAPI.Utils;

namespace WebSafeDockingAPI.Services
{
    public class GrupoRecursoService
    {
        private readonly IGrupoRecursoRepository _repository;
        private readonly IGrupoRepository _grupoRepository;

        public GrupoRecursoService(
            IGrupoRecursoRepository repository,
            IGrupoRepository grupoRepository)
        {
            _repository = repository;
            _grupoRepository = grupoRepository;
        }

        /// <summary>
        /// Busca um vinculo grupo-recurso pela chave composta.
        /// </summary>
        public async Task<GrupoRecursoResponseDTO?> GetByIdAsync(int grupoId, string recursoChave)
        {
            var entidade = await _repository.GetByIdAsync(grupoId, recursoChave);
            return entidade != null ? GrupoRecursoResponseDTO.FromGrupoRecurso(entidade) : null;
        }

        /// <summary>
        /// Cria um novo vinculo grupo-recurso.
        /// </summary>
        public async Task<GrupoRecursoResponseDTO> CriarGrupoRecursoAsync(GrupoRecursoCreateUpdateDTO dto)
        {
            var recursoChave = NormalizarRecursoChave(dto.RecursoChave);
            await ValidarGrupoAsync(dto.GrupoId);

            var existente = await _repository.GetByIdAsync(dto.GrupoId, recursoChave);
            if (existente != null)
            {
                throw new ValidationException(
                    $"Ja existe vinculo para o grupo {dto.GrupoId} com recurso '{recursoChave}'.");
            }

            var novaEntidade = dto.ToEntity();
            novaEntidade.RecursoChave = recursoChave;

            var entidadeCriada = await _repository.CreateAsync(novaEntidade);
            return GrupoRecursoResponseDTO.FromGrupoRecurso(entidadeCriada);
        }

        /// <summary>
        /// Atualiza um vinculo grupo-recurso existente.
        /// </summary>
        public async Task<GrupoRecursoResponseDTO?> AtualizarGrupoRecursoAsync(
            int grupoIdOriginal,
            string recursoChaveOriginal,
            GrupoRecursoCreateUpdateDTO dto)
        {
            var recursoChaveOriginalNormalizada = NormalizarRecursoChave(recursoChaveOriginal);
            var recursoChaveNova = NormalizarRecursoChave(dto.RecursoChave);

            var entidadeExistente = await _repository.GetByIdAsync(
                grupoIdOriginal,
                recursoChaveOriginalNormalizada);

            if (entidadeExistente == null) return null;

            await ValidarGrupoAsync(dto.GrupoId);

            var chaveFoiAlterada = grupoIdOriginal != dto.GrupoId
                || !string.Equals(recursoChaveOriginalNormalizada, recursoChaveNova, StringComparison.Ordinal);

            if (chaveFoiAlterada)
            {
                var conflito = await _repository.GetByIdAsync(dto.GrupoId, recursoChaveNova);
                if (conflito != null)
                {
                    throw new ValidationException(
                        $"Ja existe vinculo para o grupo {dto.GrupoId} com recurso '{recursoChaveNova}'.");
                }
            }

            if (chaveFoiAlterada)
            {
                await _repository.DeleteAsync(grupoIdOriginal, recursoChaveOriginalNormalizada);

                var novaEntidade = dto.ToEntity();
                novaEntidade.RecursoChave = recursoChaveNova;

                var recriada = await _repository.CreateAsync(novaEntidade);
                return GrupoRecursoResponseDTO.FromGrupoRecurso(recriada);
            }

            entidadeExistente.GrupoId = dto.GrupoId;
            entidadeExistente.RecursoChave = recursoChaveNova;

            var atualizado = await _repository.UpdateAsync(entidadeExistente);
            if (!atualizado) return null;

            return GrupoRecursoResponseDTO.FromGrupoRecurso(entidadeExistente);
        }

        /// <summary>
        /// Deleta um vinculo grupo-recurso pela chave composta.
        /// </summary>
        public async Task<bool> DeletarGrupoRecursoAsync(int grupoId, string recursoChave)
        {
            var recursoChaveNormalizada = NormalizarRecursoChave(recursoChave);
            return await _repository.DeleteAsync(grupoId, recursoChaveNormalizada);
        }

        /// <summary>
        /// Busca vinculos grupo-recurso com paginacao estilo Supabase.
        /// </summary>
        public async Task<PaginatedResponse<GrupoRecursoResponseDTO>> SearchGruposRecursosAsync(
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

        private async Task ValidarGrupoAsync(int grupoId)
        {
            var grupo = await _grupoRepository.GetByIdAsync(grupoId);
            if (grupo == null)
            {
                throw new ValidationException($"Grupo com ID {grupoId} nao encontrado.");
            }
        }

        private static string NormalizarRecursoChave(string recursoChave)
        {
            var chave = recursoChave?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(chave))
            {
                throw new ValidationException("A chave do recurso e obrigatoria.");
            }

            return chave;
        }
    }
}
