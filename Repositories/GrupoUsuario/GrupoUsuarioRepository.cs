using Microsoft.EntityFrameworkCore;
using WebSafeDockingAPI.Data;
using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Repositories
{
    public class GrupoUsuarioRepository : IGrupoUsuarioRepository
    {
        private readonly DataDbContext _context;

        public GrupoUsuarioRepository(DataDbContext context)
        {
            _context = context;
        }

        public async Task<GrupoUsuario?> GetByIdAsync(int grupoId, int usuarioId)
        {
            return await _context.GrupoUsuarios.FindAsync(grupoId, usuarioId);
        }

        public async Task<GrupoUsuario> CreateAsync(GrupoUsuario grupoUsuario)
        {
            _context.GrupoUsuarios.Add(grupoUsuario);
            await _context.SaveChangesAsync();
            return grupoUsuario;
        }

        public async Task<bool> UpdateAsync(GrupoUsuario grupoUsuario)
        {
            _context.GrupoUsuarios.Update(grupoUsuario);
            var rows = await _context.SaveChangesAsync();
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int grupoId, int usuarioId)
        {
            var grupoUsuario = await _context.GrupoUsuarios.FindAsync(grupoId, usuarioId);
            if (grupoUsuario == null) return false;

            _context.GrupoUsuarios.Remove(grupoUsuario);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Busca vinculos grupo-usuario por termo de busca com paginacao e ordenacao.
        /// </summary>
        public async Task<(List<GrupoUsuarioResponseDTO> Items, int TotalCount)> SearchAsync(
            int page,
            int pageSize,
            string? searchTerm,
            string sortBy,
            string sortOrder)
        {
            var query = _context.GrupoUsuarios.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                int.TryParse(searchTerm, out var searchNumber);
                query = query.Where(gu =>
                    (searchNumber > 0 && gu.GrupoId == searchNumber) ||
                    (searchNumber > 0 && gu.UsuarioId == searchNumber)
                );
            }

            var totalCount = await query.CountAsync();

            query = ApplySorting(query, sortBy, sortOrder);

            var grupoUsuarios = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = grupoUsuarios.Select(GrupoUsuarioResponseDTO.FromGrupoUsuario).ToList();

            return (items, totalCount);
        }

        /// <summary>
        /// Aplica ordenacao dinamica a query.
        /// </summary>
        private IQueryable<GrupoUsuario> ApplySorting(
            IQueryable<GrupoUsuario> query,
            string sortBy,
            string sortOrder)
        {
            var isDescending = sortOrder?.ToLower() == "desc";

            return sortBy?.ToLower() switch
            {
                "grupoid" => isDescending
                    ? query.OrderByDescending(gu => gu.GrupoId)
                    : query.OrderBy(gu => gu.GrupoId),
                "usuarioid" => isDescending
                    ? query.OrderByDescending(gu => gu.UsuarioId)
                    : query.OrderBy(gu => gu.UsuarioId),
                _ => query.OrderBy(gu => gu.GrupoId).ThenBy(gu => gu.UsuarioId)
            };
        }
    }
}
