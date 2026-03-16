using Microsoft.EntityFrameworkCore;
using WebSafeDockingAPI.Data;
using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Repositories
{
    public class MooringCompanyRepository : IMooringCompanyRepository
    {
        private readonly DataDbContext _context;

        public MooringCompanyRepository(DataDbContext context)
        {
            _context = context;
        }

        public async Task<MooringCompany?> GetByIdAsync(int id)
        {
            return await _context.MooringCompanies.FindAsync(id);
        }

        public async Task<MooringCompany> CreateAsync(MooringCompany mooringCompany)
        {
            _context.MooringCompanies.Add(mooringCompany);
            await _context.SaveChangesAsync();
            return mooringCompany;
        }

        public async Task<bool> UpdateAsync(MooringCompany mooringCompany)
        {
            _context.MooringCompanies.Update(mooringCompany);
            var rows = await _context.SaveChangesAsync();
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var mooringCompany = await _context.MooringCompanies.FindAsync(id);
            if (mooringCompany == null) return false;

            _context.MooringCompanies.Remove(mooringCompany);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Busca empresas de amarração por termo de busca com paginação e ordenação.
        /// </summary>
        public async Task<(List<MooringCompanyResponseDTO> Items, int TotalCount)> SearchAsync(
            int page,
            int pageSize,
            string? searchTerm,
            string sortBy,
            string sortOrder)
        {
            var query = _context.MooringCompanies.AsQueryable();

            // Aplicar filtro de busca
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                int.TryParse(searchTerm, out var searchNumber);

                query = query.Where(mc =>
                    mc.Name.Contains(searchTerm) ||
                    (searchNumber > 0 && mc.MooringCompanyID == searchNumber)
                );
            }

            // Obter total de registros antes da paginação
            var totalCount = await query.CountAsync();

            // Aplicar ordenação dinâmica
            query = ApplySorting(query, sortBy, sortOrder);

            // Aplicar paginação e converter para DTO
            var mooringCompanies = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = mooringCompanies.Select(MooringCompanyResponseDTO.FromMooringCompany).ToList();

            return (items, totalCount);
        }

        /// <summary>
        /// Aplica ordenação dinâmica à query.
        /// </summary>
        private IQueryable<MooringCompany> ApplySorting(IQueryable<MooringCompany> query, string sortBy, string sortOrder)
        {
            var isDescending = sortOrder?.ToLower() == "desc";

            return sortBy?.ToLower() switch
            {
                "id" or "mooringcompanyid" => isDescending ? query.OrderByDescending(mc => mc.MooringCompanyID) : query.OrderBy(mc => mc.MooringCompanyID),
                "name" or "nome" => isDescending ? query.OrderByDescending(mc => mc.Name) : query.OrderBy(mc => mc.Name),
                _ => query.OrderBy(mc => mc.Name) // Ordenação padrão por Name
            };
        }
    }
}
