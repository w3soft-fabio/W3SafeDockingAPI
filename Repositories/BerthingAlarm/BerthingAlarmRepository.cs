using Microsoft.EntityFrameworkCore;
using WebSafeDockingAPI.Data;
using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Repositories
{
    public class BerthingAlarmRepository : IBerthingAlarmRepository
    {
        private readonly DataDbContext _context;

        public BerthingAlarmRepository(DataDbContext context)
        {
            _context = context;
        }

        public async Task<BerthingAlarm?> GetByIdAsync(int id)
        {
            return await _context.BerthingAlarms.FindAsync(id);
        }

        public async Task<BerthingAlarm> CreateAsync(BerthingAlarm alarm)
        {
            _context.BerthingAlarms.Add(alarm);
            await _context.SaveChangesAsync();
            return alarm;
        }

        public async Task<bool> UpdateAsync(BerthingAlarm alarm)
        {
            _context.BerthingAlarms.Update(alarm);
            var rows = await _context.SaveChangesAsync();
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var alarm = await _context.BerthingAlarms.FindAsync(id);
            if (alarm == null) return false;

            _context.BerthingAlarms.Remove(alarm);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Busca alarmes por termo de busca com paginação e ordenação.
        /// </summary>
        public async Task<(List<BerthingAlarmResponseDTO> Items, int TotalCount)> SearchAsync(
            int page,
            int pageSize,
            string? searchTerm,
            string sortBy,
            string sortOrder)
        {
            var query = _context.BerthingAlarms.AsQueryable();

            // Aplicar filtro de busca
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                // Tentar converter para número para busca por ID
                int.TryParse(searchTerm, out var searchNumber);

                query = query.Where(a =>
                    (searchNumber > 0 && a.Id == searchNumber)
                );
            }

            // Obter total de registros antes da paginação
            var totalCount = await query.CountAsync();

            // Aplicar ordenação dinâmica
            query = ApplySorting(query, sortBy, sortOrder);

            // Aplicar paginação e converter para DTO
            var alarms = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = alarms.Select(BerthingAlarmResponseDTO.FromBerthingAlarm).ToList();

            return (items, totalCount);
        }

        /// <summary>
        /// Aplica ordenação dinâmica à query.
        /// </summary>
        private IQueryable<BerthingAlarm> ApplySorting(
            IQueryable<BerthingAlarm> query,
            string sortBy,
            string sortOrder)
        {
            var isDescending = sortOrder?.ToLower() == "desc";

            return sortBy?.ToLower() switch
            {
                "id" => isDescending ? query.OrderByDescending(a => a.Id) : query.OrderBy(a => a.Id),
                "maxshipangle" => isDescending ? query.OrderByDescending(a => a.MaxShipAngle) : query.OrderBy(a => a.MaxShipAngle),
                "zcminrange" => isDescending ? query.OrderByDescending(a => a.ZcMinRange) : query.OrderBy(a => a.ZcMinRange),
                "zcmaxrange" => isDescending ? query.OrderByDescending(a => a.ZcMaxRange) : query.OrderBy(a => a.ZcMaxRange),
                "zcspeedwarning" => isDescending ? query.OrderByDescending(a => a.ZcSpeedWarning) : query.OrderBy(a => a.ZcSpeedWarning),
                "zcspeedalarm" => isDescending ? query.OrderByDescending(a => a.ZcSpeedAlarm) : query.OrderBy(a => a.ZcSpeedAlarm),
                "zmminrange" => isDescending ? query.OrderByDescending(a => a.ZmMinRange) : query.OrderBy(a => a.ZmMinRange),
                "zmmaxrange" => isDescending ? query.OrderByDescending(a => a.ZmMaxRange) : query.OrderBy(a => a.ZmMaxRange),
                "zmspeedwarning" => isDescending ? query.OrderByDescending(a => a.ZmSpeedWarning) : query.OrderBy(a => a.ZmSpeedWarning),
                "zmspeedalarm" => isDescending ? query.OrderByDescending(a => a.ZmSpeedAlarm) : query.OrderBy(a => a.ZmSpeedAlarm),
                "zfminrange" => isDescending ? query.OrderByDescending(a => a.ZfMinRange) : query.OrderBy(a => a.ZfMinRange),
                "zfmaxrange" => isDescending ? query.OrderByDescending(a => a.ZfMaxRange) : query.OrderBy(a => a.ZfMaxRange),
                "zfspeedwarning" => isDescending ? query.OrderByDescending(a => a.ZfSpeedWarning) : query.OrderBy(a => a.ZfSpeedWarning),
                "zfspeedalarm" => isDescending ? query.OrderByDescending(a => a.ZfSpeedAlarm) : query.OrderBy(a => a.ZfSpeedAlarm),
                _ => query.OrderBy(a => a.Id) // Ordenação padrão por Id
            };
        }
    }
}
