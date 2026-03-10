using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Repositories
{
    public interface IBerthingAlarmRepository
    {
        Task<BerthingAlarm?> GetByIdAsync(int id);
        Task<BerthingAlarm> CreateAsync(BerthingAlarm alarm);
        Task<bool> UpdateAsync(BerthingAlarm alarm);
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Busca alarmes por termo com paginação e ordenação.
        /// </summary>
        Task<(List<BerthingAlarmResponseDTO> Items, int TotalCount)> SearchAsync(
            int page,
            int pageSize,
            string? searchTerm,
            string sortBy,
            string sortOrder);
    }
}
