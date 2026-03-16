using WebSafeDockingAPI.Models;
using WebSafeDockingAPI.Models.Common;
using WebSafeDockingAPI.Repositories;
using WebSafeDockingAPI.Utils;

namespace WebSafeDockingAPI.Services
{
    public class ShippingAgencyService
    {
        private readonly IShippingAgencyRepository _repository;

        public ShippingAgencyService(IShippingAgencyRepository repository)
        {
            _repository = repository;
        }

        public async Task<ShippingAgencyResponseDTO?> GetByIdAsync(int id)
        {
            var agency = await _repository.GetByIdAsync(id);
            return agency != null ? ShippingAgencyResponseDTO.FromShippingAgency(agency) : null;
        }

        public async Task<ShippingAgencyResponseDTO> CriarShippingAgencyAsync(ShippingAgencyCreateUpdateDTO dto)
        {
            var novaAgency = dto.ToEntity();
            var agencyCriada = await _repository.CreateAsync(novaAgency);
            return ShippingAgencyResponseDTO.FromShippingAgency(agencyCriada);
        }

        public async Task<ShippingAgencyResponseDTO?> AtualizarShippingAgencyAsync(int id, ShippingAgencyCreateUpdateDTO dto)
        {
            var agencyExistente = await _repository.GetByIdAsync(id);

            if (agencyExistente == null) return null;

            agencyExistente.Name = dto.Name;

            var atualizado = await _repository.UpdateAsync(agencyExistente);

            if (!atualizado) return null;

            return ShippingAgencyResponseDTO.FromShippingAgency(agencyExistente);
        }

        public async Task<bool> DeletarShippingAgencyAsync(int id) =>
            await _repository.DeleteAsync(id);

        public async Task<PaginatedResponse<ShippingAgencyResponseDTO>> SearchShippingAgenciesAsync(
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
