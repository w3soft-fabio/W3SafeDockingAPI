using WebSafeDockingAPI.Models;
using WebSafeDockingAPI.Models.Common;
using WebSafeDockingAPI.Repositories;
using WebSafeDockingAPI.Utils;

namespace WebSafeDockingAPI.Services
{
    public class MooringCompanyService
    {
        private readonly IMooringCompanyRepository _repository;

        public MooringCompanyService(IMooringCompanyRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Busca uma empresa de amarração pelo ID.
        /// </summary>
        public async Task<MooringCompanyResponseDTO?> GetByIdAsync(int id)
        {
            var mooringCompany = await _repository.GetByIdAsync(id);
            return mooringCompany != null ? MooringCompanyResponseDTO.FromMooringCompany(mooringCompany) : null;
        }

        /// <summary>
        /// Cria uma nova empresa de amarração.
        /// </summary>
        public async Task<MooringCompanyResponseDTO> CriarMooringCompanyAsync(MooringCompanyCreateUpdateDTO dto)
        {
            var novoMooringCompany = dto.ToEntity();
            var mooringCompanyCriado = await _repository.CreateAsync(novoMooringCompany);
            return MooringCompanyResponseDTO.FromMooringCompany(mooringCompanyCriado);
        }

        /// <summary>
        /// Atualiza uma empresa de amarração existente.
        /// </summary>
        public async Task<MooringCompanyResponseDTO?> AtualizarMooringCompanyAsync(int id, MooringCompanyCreateUpdateDTO dto)
        {
            var mooringCompanyExistente = await _repository.GetByIdAsync(id);

            if (mooringCompanyExistente == null) return null;

            mooringCompanyExistente.Name = dto.Name;

            var atualizado = await _repository.UpdateAsync(mooringCompanyExistente);

            if (!atualizado) return null;

            return MooringCompanyResponseDTO.FromMooringCompany(mooringCompanyExistente);
        }

        /// <summary>
        /// Deleta uma empresa de amarração pelo ID.
        /// </summary>
        public async Task<bool> DeletarMooringCompanyAsync(int id) =>
            await _repository.DeleteAsync(id);

        /// <summary>
        /// Busca empresas de amarração por termo de busca com paginação estilo Supabase.
        /// </summary>
        public async Task<PaginatedResponse<MooringCompanyResponseDTO>> SearchMooringCompaniesAsync(
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
