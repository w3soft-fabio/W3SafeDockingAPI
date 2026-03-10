using Microsoft.EntityFrameworkCore;
using WebSafeDockingAPI.Data;
using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly DataDbContext _context;

        public UsuarioRepository(DataDbContext context)
        {
            _context = context;
        }

        public async Task<Usuario?> GetByIdAsync(int id)
        {
            return await _context.Usuarios.FindAsync(id);
        }

        public async Task<Usuario> CreateAsync(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }

        public async Task<bool> UpdateAsync(Usuario usuario)
        {
            _context.Usuarios.Update(usuario);
            var rows = await _context.SaveChangesAsync();
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return false;

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Busca usuários por termo de busca com paginação e ordenação.
        /// </summary>
        public async Task<(List<UsuarioResponseDTO> Items, int TotalCount)> SearchAsync(
            int page,
            int pageSize,
            string? searchTerm,
            string sortBy,
            string sortOrder)
        {
            var query = _context.Usuarios.AsQueryable();

            // Aplicar filtro de busca
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(u =>
                    (u.Nome != null && u.Nome.Contains(searchTerm)) ||
                    (u.Email != null && u.Email.Contains(searchTerm)) ||
                    (u.Cpf != null && u.Cpf.Contains(searchTerm)) ||
                    (u.NivelAcesso != null && u.NivelAcesso.Contains(searchTerm))
                );
            }

            // Obter total de registros antes da paginação
            var totalCount = await query.CountAsync();

            // Aplicar ordenação dinâmica
            query = ApplySorting(query, sortBy, sortOrder);

            // Aplicar paginação e converter para DTO
            var usuarios = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = usuarios.Select(UsuarioResponseDTO.FromUsuario).ToList();

            return (items, totalCount);
        }

        /// <summary>
        /// Aplica ordenação dinâmica à query.
        /// </summary>
        private IQueryable<Usuario> ApplySorting(IQueryable<Usuario> query, string sortBy, string sortOrder)
        {
            var isDescending = sortOrder?.ToLower() == "desc";

            return sortBy?.ToLower() switch
            {
                "nome" or "name" => isDescending
                    ? query.OrderByDescending(u => u.Nome)
                    : query.OrderBy(u => u.Nome),
                "email" => isDescending
                    ? query.OrderByDescending(u => u.Email)
                    : query.OrderBy(u => u.Email),
                "cpf" => isDescending
                    ? query.OrderByDescending(u => u.Cpf)
                    : query.OrderBy(u => u.Cpf),
                "nivelacessso" or "nivelacess" or "nivel" => isDescending
                    ? query.OrderByDescending(u => u.NivelAcesso)
                    : query.OrderBy(u => u.NivelAcesso),
                "ativo" => isDescending
                    ? query.OrderByDescending(u => u.Ativo)
                    : query.OrderBy(u => u.Ativo),
                "id" => isDescending
                    ? query.OrderByDescending(u => u.Id)
                    : query.OrderBy(u => u.Id),
                _ => query.OrderBy(u => u.Nome) // Ordenação padrão por Nome
            };
        }
    }
}
