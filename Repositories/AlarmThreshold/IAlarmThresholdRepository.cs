using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Repositories
{
    public interface IAlarmThresholdRepository
    {
        Task<AlarmThreshold?> GetByIdAsync(int id);
        Task<AlarmThreshold> CreateAsync(AlarmThreshold alarmThreshold);
        Task<bool> UpdateAsync(AlarmThreshold alarmThreshold);
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Busca alarm thresholds por termo com paginação e ordenação.
        /// </summary>
        Task<(List<AlarmThresholdResponseDTO> Items, int TotalCount)> SearchAsync(
            int page,
            int pageSize,
            string? searchTerm,
            string sortBy,
            string sortOrder);
    }
}
