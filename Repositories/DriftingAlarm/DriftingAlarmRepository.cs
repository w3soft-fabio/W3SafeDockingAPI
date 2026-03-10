using Microsoft.EntityFrameworkCore;
using WebSafeDockingAPI.Data;
using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Repositories
{
    public class DriftingAlarmRepository : IDriftingAlarmRepository
    {
        private readonly DataDbContext _context;

        public DriftingAlarmRepository(DataDbContext context)
        {
            _context = context;
        }

        public async Task<DriftingAlarm?> GetByIdAsync(int id)
        {
            return await _context.DriftingAlarms.FindAsync(id);
        }

        public async Task<DriftingAlarm> CreateAsync(DriftingAlarm alarm)
        {
            _context.DriftingAlarms.Add(alarm);
            await _context.SaveChangesAsync();
            return alarm;
        }

        public async Task<bool> UpdateAsync(DriftingAlarm alarm)
        {
            _context.DriftingAlarms.Update(alarm);
            var rows = await _context.SaveChangesAsync();
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var alarm = await _context.DriftingAlarms.FindAsync(id);
            if (alarm == null) return false;

            _context.DriftingAlarms.Remove(alarm);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Busca alarmes de deriva com paginação e ordenação.
        /// </summary>
        public async Task<(List<DriftingAlarmResponseDTO> Items, int TotalCount)> SearchAsync(
            int page,
            int pageSize,
            string? searchTerm,
            string sortBy,
            string sortOrder)
        {
            var query = _context.DriftingAlarms.AsQueryable();

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

            var items = alarms.Select(DriftingAlarmResponseDTO.FromDriftingAlarm).ToList();

            return (items, totalCount);
        }

        /// <summary>
        /// Aplica ordenação dinâmica à query.
        /// </summary>
        private IQueryable<DriftingAlarm> ApplySorting(
            IQueryable<DriftingAlarm> query,
            string sortBy,
            string sortOrder)
        {
            var isDescending = sortOrder?.ToLower() == "desc";

            return sortBy?.ToLower() switch
            {
                "id" => isDescending ? query.OrderByDescending(a => a.Id) : query.OrderBy(a => a.Id),
                "maxshipangle" => isDescending ? query.OrderByDescending(a => a.MaxShipAngle) : query.OrderBy(a => a.MaxShipAngle),
                "outboundwarning" => isDescending ? query.OrderByDescending(a => a.OutboundWarning) : query.OrderBy(a => a.OutboundWarning),
                "outboundalarm" => isDescending ? query.OrderByDescending(a => a.OutboundAlarm) : query.OrderBy(a => a.OutboundAlarm),
                "inboundwarning" => isDescending ? query.OrderByDescending(a => a.InboundWarning) : query.OrderBy(a => a.InboundWarning),
                "inboundalarm" => isDescending ? query.OrderByDescending(a => a.InboundAlarm) : query.OrderBy(a => a.InboundAlarm),
                _ => query.OrderBy(a => a.Id) // Ordenação padrão por Id
            };
        }
    }
}
