using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Repositories
{
    public interface IBerthingRepository
    {
        Task<Berthing?> GetByIdAsync(int id);
        Task<Berthing> CreateAsync(Berthing berthing);
        Task<bool> UpdateAsync(Berthing berthing);
        Task<bool> DeleteAsync(int id);

        Task<(List<BerthingResponseDTO> Items, int TotalCount)> SearchAsync(
            int page,
            int pageSize,
            string? searchTerm,
            string sortBy,
            string sortOrder);
    }
}
