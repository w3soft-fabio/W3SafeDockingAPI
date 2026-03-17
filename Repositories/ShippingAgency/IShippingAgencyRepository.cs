using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Repositories
{
    public interface IShippingAgencyRepository
    {
        Task<ShippingAgency?> GetByIdAsync(int id);
        Task<ShippingAgency> CreateAsync(ShippingAgency agency);
        Task<bool> UpdateAsync(ShippingAgency agency);
        Task<bool> DeleteAsync(int id);

        Task<(List<ShippingAgencyResponseDTO> Items, int TotalCount)> SearchAsync(
            int page,
            int pageSize,
            string? searchTerm,
            string sortBy,
            string sortOrder);
    }
}
