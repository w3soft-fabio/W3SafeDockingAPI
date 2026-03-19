using Microsoft.EntityFrameworkCore;
using WebSafeDockingAPI.Data;
using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Repositories
{
    public class GrupoRecursoRepository : IGrupoRecursoRepository
    {
        private readonly DataDbContext _context;

        public GrupoRecursoRepository(DataDbContext context)
        {
            _context = context;
        }

        public async Task<GrupoRecurso?> GetByIdAsync(int grupoId, string recursoChave)
        {
            return await _context.GrupoRecursos.FindAsync(grupoId, recursoChave);
        }

        public async Task<bool> UsuarioTemRecursoAsync(int usuarioId, string recursoChave)
        {
            return await _context.GrupoUsuarios
                .Where(gu => gu.UsuarioId == usuarioId)
                .Join(
                    _context.GrupoRecursos.Where(gr => gr.RecursoChave == recursoChave),
                    gu => gu.GrupoId,
                    gr => gr.GrupoId,
                    (_, _) => 1)
                .AnyAsync();
        }

        public async Task<GrupoRecurso> CreateAsync(GrupoRecurso grupoRecurso)
        {
            _context.GrupoRecursos.Add(grupoRecurso);
            await _context.SaveChangesAsync();
            return grupoRecurso;
        }

        public async Task<bool> UpdateAsync(GrupoRecurso grupoRecurso)
        {
            _context.GrupoRecursos.Update(grupoRecurso);
            var rows = await _context.SaveChangesAsync();
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int grupoId, string recursoChave)
        {
            var grupoRecurso = await _context.GrupoRecursos.FindAsync(grupoId, recursoChave);
            if (grupoRecurso == null) return false;

            _context.GrupoRecursos.Remove(grupoRecurso);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Busca vinculos grupo-recurso por termo de busca com paginacao e ordenacao.
        /// </summary>
        public async Task<(List<GrupoRecursoResponseDTO> Items, int TotalCount)> SearchAsync(
            int page,
            int pageSize,
            string? searchTerm,
            string sortBy,
            string sortOrder)
        {
            var query = _context.GrupoRecursos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                int.TryParse(searchTerm, out var searchNumber);
                query = query.Where(gr =>
                    (gr.RecursoChave != null && gr.RecursoChave.Contains(searchTerm)) ||
                    (searchNumber > 0 && gr.GrupoId == searchNumber)
                );
            }

            var totalCount = await query.CountAsync();

            query = ApplySorting(query, sortBy, sortOrder);

            var grupoRecursos = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = grupoRecursos.Select(GrupoRecursoResponseDTO.FromGrupoRecurso).ToList();

            return (items, totalCount);
        }

        /// <summary>
        /// Aplica ordenacao dinamica a query.
        /// </summary>
        private IQueryable<GrupoRecurso> ApplySorting(
            IQueryable<GrupoRecurso> query,
            string sortBy,
            string sortOrder)
        {
            var isDescending = sortOrder?.ToLower() == "desc";

            return sortBy?.ToLower() switch
            {
                "grupoid" or "id" => isDescending
                    ? query.OrderByDescending(gr => gr.GrupoId)
                    : query.OrderBy(gr => gr.GrupoId),
                "recursochave" or "chave" => isDescending
                    ? query.OrderByDescending(gr => gr.RecursoChave)
                    : query.OrderBy(gr => gr.RecursoChave),
                _ => query.OrderBy(gr => gr.GrupoId).ThenBy(gr => gr.RecursoChave)
            };
        }
    }
}
