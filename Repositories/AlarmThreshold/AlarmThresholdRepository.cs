using Microsoft.EntityFrameworkCore;
using WebSafeDockingAPI.Data;
using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Repositories
{
    public class AlarmThresholdRepository : IAlarmThresholdRepository
    {
        private readonly DataDbContext _context;

        public AlarmThresholdRepository(DataDbContext context)
        {
            _context = context;
        }

        public async Task<AlarmThreshold?> GetByIdAsync(int id)
        {
            return await _context.AlarmThresholds
                .Include(a => a.BerthingAlarm)
                .Include(a => a.DriftingAlarm)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<AlarmThreshold> CreateAsync(AlarmThreshold alarmThreshold)
        {
            _context.AlarmThresholds.Add(alarmThreshold);
            await _context.SaveChangesAsync();
            return alarmThreshold;
        }

        public async Task<bool> UpdateAsync(AlarmThreshold alarmThreshold)
        {
            _context.AlarmThresholds.Update(alarmThreshold);
            var rows = await _context.SaveChangesAsync();
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var alarmThreshold = await _context.AlarmThresholds.FindAsync(id);
            if (alarmThreshold == null) return false;

            _context.AlarmThresholds.Remove(alarmThreshold);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Busca alarm thresholds por termo de busca com paginação e ordenação.
        /// </summary>
        public async Task<(List<AlarmThresholdResponseDTO> Items, int TotalCount)> SearchAsync(
            int page,
            int pageSize,
            string? searchTerm,
            string sortBy,
            string sortOrder)
        {
            var query = _context.AlarmThresholds
                .Include(a => a.BerthingAlarm)
                .Include(a => a.DriftingAlarm)
                .AsQueryable();

            // Aplicar filtro de busca
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                int.TryParse(searchTerm, out var searchNumber);

                query = query.Where(a =>
                    (a.Name != null && a.Name.Contains(searchTerm)) ||
                    (searchNumber > 0 && a.Id == searchNumber)
                );
            }

            // Obter total de registros antes da paginação
            var totalCount = await query.CountAsync();

            // Aplicar ordenação dinâmica
            query = ApplySorting(query, sortBy, sortOrder);

            // Aplicar paginação e converter para DTO
            var alarmThresholds = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = alarmThresholds.Select(AlarmThresholdResponseDTO.FromAlarmThreshold).ToList();

            return (items, totalCount);
        }

        /// <summary>
        /// Aplica ordenação dinâmica à query.
        /// </summary>
        private IQueryable<AlarmThreshold> ApplySorting(IQueryable<AlarmThreshold> query, string sortBy, string sortOrder)
        {
            var isDescending = sortOrder?.ToLower() == "desc";

            return sortBy?.ToLower() switch
            {
                "id" => isDescending ? query.OrderByDescending(a => a.Id) : query.OrderBy(a => a.Id),
                "name" or "nome" => isDescending ? query.OrderByDescending(a => a.Name) : query.OrderBy(a => a.Name),
                "berthingid" => isDescending ? query.OrderByDescending(a => a.BerthingID) : query.OrderBy(a => a.BerthingID),
                "driftingid" => isDescending ? query.OrderByDescending(a => a.DriftingID) : query.OrderBy(a => a.DriftingID),
                "mindwt" => isDescending ? query.OrderByDescending(a => a.MinDwt) : query.OrderBy(a => a.MinDwt),
                "maxdwt" => isDescending ? query.OrderByDescending(a => a.MaxDwt) : query.OrderBy(a => a.MaxDwt),
                _ => query.OrderBy(a => a.Name) // Ordenação padrão por Name
            };
        }
    }
}
