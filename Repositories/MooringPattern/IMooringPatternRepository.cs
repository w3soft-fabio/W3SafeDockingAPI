using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Repositories
{
    public interface IMooringPatternRepository
    {
        Task<MooringPattern?> GetByIdAsync(long id);
        Task<MooringPattern> CreateAsync(MooringPattern mooringPattern);
        Task<bool> UpdateAsync(MooringPattern mooringPattern);
        Task<bool> DeleteAsync(long id);

        /// <summary>
        /// Busca mooring patterns por termo com paginação e ordenação.
        /// </summary>
        Task<(List<MooringPatternResponseDTO> Items, int TotalCount)> SearchAsync(
            int page,
            int pageSize,
            string? searchTerm,
            string sortBy,
            string sortOrder);
    }
}
