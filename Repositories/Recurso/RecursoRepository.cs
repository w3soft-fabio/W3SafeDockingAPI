using Microsoft.EntityFrameworkCore;
using WebSafeDockingAPI.Data;
using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Repositories
{
    public class RecursoRepository : IRecursoRepository
    {
        private readonly DataDbContext _context;

        public RecursoRepository(DataDbContext context)
        {
            _context = context;
        }

        public async Task<Recurso?> GetByIdAsync(string recursoChave)
        {
            return await _context.Recursos.FindAsync(recursoChave);
        }

        public async Task<Recurso> CreateAsync(Recurso recurso)
        {
            _context.Recursos.Add(recurso);
            await _context.SaveChangesAsync();
            return recurso;
        }

        public async Task<bool> UpdateAsync(Recurso recurso)
        {
            _context.Recursos.Update(recurso);
            var rows = await _context.SaveChangesAsync();
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(string recursoChave)
        {
            var recurso = await _context.Recursos.FindAsync(recursoChave);
            if (recurso == null) return false;

            _context.Recursos.Remove(recurso);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Busca recursos por termo de busca com paginacao e ordenacao.
        /// </summary>
        public async Task<(List<RecursoResponseDTO> Items, int TotalCount)> SearchAsync(
            int page,
            int pageSize,
            string? searchTerm,
            string sortBy,
            string sortOrder)
        {
            var query = _context.Recursos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(r =>
                    (r.RecursoChave != null && r.RecursoChave.Contains(searchTerm)) ||
                    (r.RecursoArea != null && r.RecursoArea.Contains(searchTerm)) ||
                    (r.RecursoDescricao != null && r.RecursoDescricao.Contains(searchTerm)) ||
                    (r.DescricaoInstrutiva != null && r.DescricaoInstrutiva.Contains(searchTerm)) ||
                    (r.UrlVideo != null && r.UrlVideo.Contains(searchTerm))
                );
            }

            var totalCount = await query.CountAsync();

            query = ApplySorting(query, sortBy, sortOrder);

            var recursos = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = recursos.Select(RecursoResponseDTO.FromRecurso).ToList();

            return (items, totalCount);
        }

        /// <summary>
        /// Aplica ordenacao dinamica a query.
        /// </summary>
        private IQueryable<Recurso> ApplySorting(IQueryable<Recurso> query, string sortBy, string sortOrder)
        {
            var isDescending = sortOrder?.ToLower() == "desc";

            return sortBy?.ToLower() switch
            {
                "recursochave" or "chave" or "id" => isDescending
                    ? query.OrderByDescending(r => r.RecursoChave)
                    : query.OrderBy(r => r.RecursoChave),
                "recursoarea" or "area" => isDescending
                    ? query.OrderByDescending(r => r.RecursoArea)
                    : query.OrderBy(r => r.RecursoArea),
                "recursodescricao" or "descricao" => isDescending
                    ? query.OrderByDescending(r => r.RecursoDescricao)
                    : query.OrderBy(r => r.RecursoDescricao),
                "descricaoinstrutiva" => isDescending
                    ? query.OrderByDescending(r => r.DescricaoInstrutiva)
                    : query.OrderBy(r => r.DescricaoInstrutiva),
                "urlvideo" => isDescending
                    ? query.OrderByDescending(r => r.UrlVideo)
                    : query.OrderBy(r => r.UrlVideo),
                _ => query.OrderBy(r => r.RecursoChave)
            };
        }
    }
}
