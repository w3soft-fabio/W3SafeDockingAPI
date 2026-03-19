using WebSafeDockingAPI.Exceptions;
using WebSafeDockingAPI.Models;
using WebSafeDockingAPI.Models.Common;
using WebSafeDockingAPI.Repositories;
using WebSafeDockingAPI.Utils;

namespace WebSafeDockingAPI.Services
{
    public class RecursoService
    {
        private readonly IRecursoRepository _repository;

        public RecursoService(IRecursoRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Busca um recurso pela chave.
        /// </summary>
        public async Task<RecursoResponseDTO?> GetByIdAsync(string recursoChave)
        {
            var chaveNormalizada = NormalizarRecursoChave(recursoChave);
            var recurso = await _repository.GetByIdAsync(chaveNormalizada);

            return recurso != null ? RecursoResponseDTO.FromRecurso(recurso) : null;
        }

        /// <summary>
        /// Cria um novo recurso.
        /// </summary>
        public async Task<RecursoResponseDTO> CriarRecursoAsync(RecursoCreateUpdateDTO dto)
        {
            var chaveNormalizada = NormalizarRecursoChave(dto.RecursoChave);

            var existente = await _repository.GetByIdAsync(chaveNormalizada);
            if (existente != null)
            {
                throw new ValidationException($"Ja existe recurso com chave '{chaveNormalizada}'.");
            }

            var novoRecurso = dto.ToEntity();
            novoRecurso.RecursoChave = chaveNormalizada;

            var recursoCriado = await _repository.CreateAsync(novoRecurso);

            return RecursoResponseDTO.FromRecurso(recursoCriado);
        }

        /// <summary>
        /// Atualiza um recurso existente.
        /// </summary>
        public async Task<RecursoResponseDTO?> AtualizarRecursoAsync(
            string recursoChaveOriginal,
            RecursoCreateUpdateDTO dto)
        {
            var chaveOriginalNormalizada = NormalizarRecursoChave(recursoChaveOriginal);
            var chaveNovaNormalizada = NormalizarRecursoChave(dto.RecursoChave);

            var recursoExistente = await _repository.GetByIdAsync(chaveOriginalNormalizada);
            if (recursoExistente == null) return null;

            var chaveFoiAlterada = !string.Equals(
                chaveOriginalNormalizada,
                chaveNovaNormalizada,
                StringComparison.Ordinal);

            if (chaveFoiAlterada)
            {
                var conflito = await _repository.GetByIdAsync(chaveNovaNormalizada);
                if (conflito != null)
                {
                    throw new ValidationException($"Ja existe recurso com chave '{chaveNovaNormalizada}'.");
                }
            }

            if (chaveFoiAlterada)
            {
                await _repository.DeleteAsync(chaveOriginalNormalizada);

                var novoRecurso = dto.ToEntity();
                novoRecurso.RecursoChave = chaveNovaNormalizada;

                var recriado = await _repository.CreateAsync(novoRecurso);
                return RecursoResponseDTO.FromRecurso(recriado);
            }

            recursoExistente.RecursoChave = chaveNovaNormalizada;
            recursoExistente.RecursoArea = dto.RecursoArea;
            recursoExistente.RecursoDescricao = dto.RecursoDescricao;
            recursoExistente.DescricaoInstrutiva = dto.DescricaoInstrutiva;
            recursoExistente.UrlVideo = dto.UrlVideo;

            var atualizado = await _repository.UpdateAsync(recursoExistente);
            if (!atualizado) return null;

            return RecursoResponseDTO.FromRecurso(recursoExistente);
        }

        /// <summary>
        /// Deleta um recurso pela chave.
        /// </summary>
        public async Task<bool> DeletarRecursoAsync(string recursoChave)
        {
            var chaveNormalizada = NormalizarRecursoChave(recursoChave);
            return await _repository.DeleteAsync(chaveNormalizada);
        }

        /// <summary>
        /// Busca recursos por termo com paginacao estilo Supabase.
        /// </summary>
        public async Task<PaginatedResponse<RecursoResponseDTO>> SearchRecursosAsync(
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
