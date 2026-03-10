using Microsoft.EntityFrameworkCore;
using WebSafeDockingAPI.Data;
using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Repositories
{
    public class ShipRepository : IShipRepository
    {
        private readonly DataDbContext _context;

        public ShipRepository(DataDbContext context)
        {
            _context = context;
        }

        public async Task<Ship?> GetByIdAsync(int id)
        {
            return await _context.Ships.FindAsync(id);
        }

        public async Task<Ship> CreateAsync(Ship ship)
        {
            _context.Ships.Add(ship);
            await _context.SaveChangesAsync();
            return ship;
        }

        public async Task<bool> UpdateAsync(Ship ship)
        {
            _context.Ships.Update(ship);
            var rows = await _context.SaveChangesAsync();
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var ship = await _context.Ships.FindAsync(id);
            if (ship == null) return false;

            _context.Ships.Remove(ship);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Busca navios por termo de busca com paginação e ordenação.
        /// </summary>
        public async Task<(List<ShipResponseDTO> Items, int TotalCount)> SearchAsync(
            int page,
            int pageSize,
            string? searchTerm,
            string sortBy,
            string sortOrder)
        {
            var query = _context.Ships.AsQueryable();

            // Aplicar filtro de busca
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                // Tentar converter para número para busca por IMO/Id
                int.TryParse(searchTerm, out var searchNumber);

                query = query.Where(s =>
                    (s.Name != null && s.Name.Contains(searchTerm)) ||
                    (searchNumber > 0 && s.Imo == searchNumber) ||
                    (searchNumber > 0 && s.Id == searchNumber)
                );
            }

            // Obter total de registros antes da paginação
            var totalCount = await query.CountAsync();

            // Aplicar ordenação dinâmica
            query = ApplySorting(query, sortBy, sortOrder);

            // Aplicar paginação e converter para DTO
            var ships = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = ships.Select(ShipResponseDTO.FromShip).ToList();

            return (items, totalCount);
        }

        /// <summary>
        /// Aplica ordenação dinâmica à query.
        /// </summary>
        private IQueryable<Ship> ApplySorting(IQueryable<Ship> query, string sortBy, string sortOrder)
        {
            var isDescending = sortOrder?.ToLower() == "desc";

            return sortBy?.ToLower() switch
            {
                "id" => isDescending ? query.OrderByDescending(s => s.Id) : query.OrderBy(s => s.Id),
                "imo" => isDescending ? query.OrderByDescending(s => s.Imo) : query.OrderBy(s => s.Imo),
                "name" or "nome" => isDescending ? query.OrderByDescending(s => s.Name) : query.OrderBy(s => s.Name),
                "length" => isDescending ? query.OrderByDescending(s => s.Length) : query.OrderBy(s => s.Length),
                "width" => isDescending ? query.OrderByDescending(s => s.Width) : query.OrderBy(s => s.Width),
                "dwt" => isDescending ? query.OrderByDescending(s => s.Dwt) : query.OrderBy(s => s.Dwt),
                "alarmid" => isDescending ? query.OrderByDescending(s => s.AlarmID) : query.OrderBy(s => s.AlarmID),
                _ => query.OrderBy(s => s.Name) // Ordenação padrão por Name
            };
        }
    }
}
