using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Repositories
{
    public interface IBerthSnapshotRepository
    {
        Task CreateManyAsync(IEnumerable<BerthSnapshot> snapshots);

        Task<(List<BerthSnapshotResponseDTO> Items, int TotalCount)> SearchAsync(
            int page,
            int pageSize,
            string? bercoId,
            DateTime? startDate,
            DateTime? endDate,
            string sortBy,
            string sortOrder);
    }
}
