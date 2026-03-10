using WebSafeDockingAPI.Models;
using WebSafeDockingAPI.Models.Common;
using WebSafeDockingAPI.Repositories;
using WebSafeDockingAPI.Utils;

namespace WebSafeDockingAPI.Services
{
    public class MooringPatternService
    {
        private readonly IMooringPatternRepository _repository;

        public MooringPatternService(IMooringPatternRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Busca um mooring pattern pelo ID.
        /// </summary>
        public async Task<MooringPatternResponseDTO?> GetByIdAsync(long id)
        {
            var mp = await _repository.GetByIdAsync(id);
            return mp != null ? MooringPatternResponseDTO.FromMooringPattern(mp) : null;
        }

        /// <summary>
        /// Cria um novo mooring pattern.
        /// </summary>
        public async Task<MooringPatternResponseDTO> CriarMooringPatternAsync(MooringPatternCreateUpdateDTO dto)
        {
            var novoMp = dto.ToEntity();
            var mpCriado = await _repository.CreateAsync(novoMp);
            return MooringPatternResponseDTO.FromMooringPattern(mpCriado);
        }

        /// <summary>
        /// Atualiza um mooring pattern existente.
        /// </summary>
        public async Task<MooringPatternResponseDTO?> AtualizarMooringPatternAsync(long id, MooringPatternCreateUpdateDTO dto)
        {
            var mpExistente = await _repository.GetByIdAsync(id);
            if (mpExistente == null) return null;

            mpExistente.Name = dto.Name;
            mpExistente.JettyID = dto.JettyID;
            mpExistente.Qrh1H1 = dto.Qrh1H1; mpExistente.Qrh1H2 = dto.Qrh1H2; mpExistente.Qrh1H3 = dto.Qrh1H3; mpExistente.Qrh1H4 = dto.Qrh1H4;
            mpExistente.Qrh2H1 = dto.Qrh2H1; mpExistente.Qrh2H2 = dto.Qrh2H2; mpExistente.Qrh2H3 = dto.Qrh2H3; mpExistente.Qrh2H4 = dto.Qrh2H4;
            mpExistente.Qrh3H1 = dto.Qrh3H1; mpExistente.Qrh3H2 = dto.Qrh3H2; mpExistente.Qrh3H3 = dto.Qrh3H3; mpExistente.Qrh3H4 = dto.Qrh3H4;
            mpExistente.Qrh4H1 = dto.Qrh4H1; mpExistente.Qrh4H2 = dto.Qrh4H2; mpExistente.Qrh4H3 = dto.Qrh4H3; mpExistente.Qrh4H4 = dto.Qrh4H4;
            mpExistente.Qrh5H1 = dto.Qrh5H1; mpExistente.Qrh5H2 = dto.Qrh5H2; mpExistente.Qrh5H3 = dto.Qrh5H3; mpExistente.Qrh5H4 = dto.Qrh5H4;
            mpExistente.Qrh6H1 = dto.Qrh6H1; mpExistente.Qrh6H2 = dto.Qrh6H2; mpExistente.Qrh6H3 = dto.Qrh6H3; mpExistente.Qrh6H4 = dto.Qrh6H4;
            mpExistente.Qrh7H1 = dto.Qrh7H1; mpExistente.Qrh7H2 = dto.Qrh7H2; mpExistente.Qrh7H3 = dto.Qrh7H3; mpExistente.Qrh7H4 = dto.Qrh7H4;
            mpExistente.Qrh8H1 = dto.Qrh8H1; mpExistente.Qrh8H2 = dto.Qrh8H2; mpExistente.Qrh8H3 = dto.Qrh8H3; mpExistente.Qrh8H4 = dto.Qrh8H4;
            mpExistente.Qrh9H1 = dto.Qrh9H1; mpExistente.Qrh9H2 = dto.Qrh9H2; mpExistente.Qrh9H3 = dto.Qrh9H3; mpExistente.Qrh9H4 = dto.Qrh9H4;
            mpExistente.Qrh10H1 = dto.Qrh10H1; mpExistente.Qrh10H2 = dto.Qrh10H2; mpExistente.Qrh10H3 = dto.Qrh10H3; mpExistente.Qrh10H4 = dto.Qrh10H4;
            mpExistente.Qrh11H1 = dto.Qrh11H1; mpExistente.Qrh11H2 = dto.Qrh11H2; mpExistente.Qrh11H3 = dto.Qrh11H3; mpExistente.Qrh11H4 = dto.Qrh11H4;
            mpExistente.Qrh12H1 = dto.Qrh12H1; mpExistente.Qrh12H2 = dto.Qrh12H2; mpExistente.Qrh12H3 = dto.Qrh12H3; mpExistente.Qrh12H4 = dto.Qrh12H4;

            var atualizado = await _repository.UpdateAsync(mpExistente);
            if (!atualizado) return null;

            return MooringPatternResponseDTO.FromMooringPattern(mpExistente);
        }

        /// <summary>
        /// Deleta um mooring pattern pelo ID.
        /// </summary>
        public async Task<bool> DeletarMooringPatternAsync(long id) =>
            await _repository.DeleteAsync(id);

        /// <summary>
        /// Busca mooring patterns com paginação estilo Supabase.
        /// </summary>
        public async Task<PaginatedResponse<MooringPatternResponseDTO>> SearchMooringPatternsAsync(
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
