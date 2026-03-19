using Microsoft.EntityFrameworkCore;
using WebSafeDockingAPI.Data;
using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Repositories
{
    public class GrupoRepository : IGrupoRepository
    {
        private readonly DataDbContext _context;

        public GrupoRepository(DataDbContext context)
        {
            _context = context;
        }

        public async Task<Grupo?> GetByIdAsync(int id)
        {
            return await _context.Grupos.FindAsync(id);
        }

        public async Task<Grupo> CreateAsync(Grupo grupo)
        {
            _context.Grupos.Add(grupo);
            await _context.SaveChangesAsync();
            return grupo;
        }

        public async Task<bool> UpdateAsync(Grupo grupo)
        {
            _context.Grupos.Update(grupo);
            var rows = await _context.SaveChangesAsync();
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var grupo = await _context.Grupos.FindAsync(id);
            if (grupo == null) return false;

            _context.Grupos.Remove(grupo);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Busca grupos por termo de busca com paginacao e ordenacao.
        /// </summary>
        public async Task<(List<GrupoResponseDTO> Items, int TotalCount)> SearchAsync(
            int page,
            int pageSize,
            string? searchTerm,
            string sortBy,
            string sortOrder)
        {
            var query = _context.Grupos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                int.TryParse(searchTerm, out var searchNumber);

                query = query.Where(g =>
                    (g.GrupoNome != null && g.GrupoNome.Contains(searchTerm)) ||
                    (g.GrupoDescricao != null && g.GrupoDescricao.Contains(searchTerm)) ||
                    (searchNumber > 0 && g.GrupoId == searchNumber) ||
                    (searchNumber > 0 && g.CriadoPorUsuarioId == searchNumber)
                );
            }

            var totalCount = await query.CountAsync();

            query = ApplySorting(query, sortBy, sortOrder);

            var grupos = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = grupos.Select(GrupoResponseDTO.FromGrupo).ToList();

            return (items, totalCount);
        }

        /// <summary>
        /// Aplica ordenacao dinamica a query.
        /// </summary>
        private IQueryable<Grupo> ApplySorting(IQueryable<Grupo> query, string sortBy, string sortOrder)
        {
            var isDescending = sortOrder?.ToLower() == "desc";

            return sortBy?.ToLower() switch
            {
                "grupoid" or "id" => isDescending
                    ? query.OrderByDescending(g => g.GrupoId)
                    : query.OrderBy(g => g.GrupoId),
                "gruponome" or "nome" or "name" => isDescending
                    ? query.OrderByDescending(g => g.GrupoNome)
                    : query.OrderBy(g => g.GrupoNome),
                "grupodescricao" or "descricao" or "description" => isDescending
                    ? query.OrderByDescending(g => g.GrupoDescricao)
                    : query.OrderBy(g => g.GrupoDescricao),
                "criadoem" or "createdat" => isDescending
                    ? query.OrderByDescending(g => g.CriadoEm)
                    : query.OrderBy(g => g.CriadoEm),
                "criadoporusuarioid" or "usuarioid" => isDescending
                    ? query.OrderByDescending(g => g.CriadoPorUsuarioId)
                    : query.OrderBy(g => g.CriadoPorUsuarioId),
                _ => query.OrderBy(g => g.GrupoNome)
            };
        }
    }
}
