using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Repositories
{
    public interface IDriftingAlarmRepository
    {
        Task<DriftingAlarm?> GetByIdAsync(int id);
        Task<DriftingAlarm> CreateAsync(DriftingAlarm alarm);
        Task<bool> UpdateAsync(DriftingAlarm alarm);
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Busca alarmes de deriva com paginação e ordenação.
        /// </summary>
        Task<(List<DriftingAlarmResponseDTO> Items, int TotalCount)> SearchAsync(
            int page,
            int pageSize,
            string? searchTerm,
            string sortBy,
            string sortOrder);
    }
}
