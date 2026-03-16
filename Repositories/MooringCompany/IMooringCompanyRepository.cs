using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Repositories
{
    public interface IMooringCompanyRepository
    {
        Task<MooringCompany?> GetByIdAsync(int id);
        Task<MooringCompany> CreateAsync(MooringCompany mooringCompany);
        Task<bool> UpdateAsync(MooringCompany mooringCompany);
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Busca empresas de amarração por termo com paginação e ordenação.
        /// </summary>
        Task<(List<MooringCompanyResponseDTO> Items, int TotalCount)> SearchAsync(
            int page,
            int pageSize,
            string? searchTerm,
            string sortBy,
            string sortOrder);
    }
}
