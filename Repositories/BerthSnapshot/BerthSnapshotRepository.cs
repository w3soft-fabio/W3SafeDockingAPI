using Microsoft.EntityFrameworkCore;
using WebSafeDockingAPI.Data;
using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Repositories
{
    public class BerthSnapshotRepository : IBerthSnapshotRepository
    {
        private readonly DataDbContext _context;

        public BerthSnapshotRepository(DataDbContext context)
        {
            _context = context;
        }

        public async Task CreateManyAsync(IEnumerable<BerthSnapshot> snapshots)
        {
            _context.BerthSnapshots.AddRange(snapshots);
            await _context.SaveChangesAsync();
        }

        public async Task<(List<BerthSnapshotResponseDTO> Items, int TotalCount)> SearchAsync(
            int page,
            int pageSize,
            string? bercoId,
            DateTime? startDate,
            DateTime? endDate,
            string sortBy,
            string sortOrder)
        {
            var query = _context.BerthSnapshots.AsQueryable();

            if (!string.IsNullOrWhiteSpace(bercoId))
                query = query.Where(s => s.BercoId == bercoId);

            if (startDate.HasValue)
                query = query.Where(s => s.CapturedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(s => s.CapturedAt <= endDate.Value);

            var totalCount = await query.CountAsync();

            query = ApplySorting(query, sortBy, sortOrder);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = items.Select(BerthSnapshotResponseDTO.FromEntity).ToList();

            return (dtos, totalCount);
        }

        private static IQueryable<BerthSnapshot> ApplySorting(
            IQueryable<BerthSnapshot> query, string sortBy, string sortOrder)
        {
            var isDescending = sortOrder?.ToLower() == "desc";

            return sortBy?.ToLower() switch
            {
                "id" => isDescending ? query.OrderByDescending(s => s.Id) : query.OrderBy(s => s.Id),
                "berco_id" or "bercoid" => isDescending ? query.OrderByDescending(s => s.BercoId) : query.OrderBy(s => s.BercoId),
                _ => isDescending ? query.OrderByDescending(s => s.CapturedAt) : query.OrderBy(s => s.CapturedAt),
            };
        }
    }
}
