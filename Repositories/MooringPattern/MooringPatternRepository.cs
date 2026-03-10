using Microsoft.EntityFrameworkCore;
using WebSafeDockingAPI.Data;
using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Repositories
{
    public class MooringPatternRepository : IMooringPatternRepository
    {
        private readonly DataDbContext _context;

        public MooringPatternRepository(DataDbContext context)
        {
            _context = context;
        }

        public async Task<MooringPattern?> GetByIdAsync(long id)
        {
            return await _context.MooringPatterns.FindAsync(id);
        }

        public async Task<MooringPattern> CreateAsync(MooringPattern mooringPattern)
        {
            _context.MooringPatterns.Add(mooringPattern);
            await _context.SaveChangesAsync();
            return mooringPattern;
        }

        public async Task<bool> UpdateAsync(MooringPattern mooringPattern)
        {
            _context.MooringPatterns.Update(mooringPattern);
            var rows = await _context.SaveChangesAsync();
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var mooringPattern = await _context.MooringPatterns.FindAsync(id);
            if (mooringPattern == null) return false;

            _context.MooringPatterns.Remove(mooringPattern);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Busca mooring patterns por termo de busca com paginação e ordenação.
        /// </summary>
        public async Task<(List<MooringPatternResponseDTO> Items, int TotalCount)> SearchAsync(
            int page,
            int pageSize,
            string? searchTerm,
            string sortBy,
            string sortOrder)
        {
            var query = _context.MooringPatterns.AsQueryable();

            // Aplicar filtro de busca
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(mp => mp.Name.Contains(searchTerm));
            }

            // Obter total de registros antes da paginação
            var totalCount = await query.CountAsync();

            // Aplicar ordenação dinâmica
            query = ApplySorting(query, sortBy, sortOrder);

            // Aplicar paginação e converter para DTO
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = items.Select(MooringPatternResponseDTO.FromMooringPattern).ToList();

            return (result, totalCount);
        }

        /// <summary>
        /// Aplica ordenação dinâmica à query.
        /// </summary>
        private IQueryable<MooringPattern> ApplySorting(
            IQueryable<MooringPattern> query,
            string sortBy,
            string sortOrder)
        {
            var isDescending = sortOrder?.ToLower() == "desc";

            return sortBy?.ToLower() switch
            {
                "id" => isDescending ? query.OrderByDescending(mp => mp.Id) : query.OrderBy(mp => mp.Id),
                "name" or "nome" => isDescending ? query.OrderByDescending(mp => mp.Name) : query.OrderBy(mp => mp.Name),
                "jettyid" => isDescending ? query.OrderByDescending(mp => mp.JettyID) : query.OrderBy(mp => mp.JettyID),
                _ => query.OrderBy(mp => mp.Name) // Ordenação padrão por Name
            };
        }
    }
}
