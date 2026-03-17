using Microsoft.EntityFrameworkCore;
using WebSafeDockingAPI.Data;
using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Repositories
{
    public class ShippingAgencyRepository : IShippingAgencyRepository
    {
        private readonly DataDbContext _context;

        public ShippingAgencyRepository(DataDbContext context)
        {
            _context = context;
        }

        public async Task<ShippingAgency?> GetByIdAsync(int id)
        {
            return await _context.ShippingAgencies.FindAsync(id);
        }

        public async Task<ShippingAgency> CreateAsync(ShippingAgency agency)
        {
            _context.ShippingAgencies.Add(agency);
            await _context.SaveChangesAsync();
            return agency;
        }

        public async Task<bool> UpdateAsync(ShippingAgency agency)
        {
            _context.ShippingAgencies.Update(agency);
            var rows = await _context.SaveChangesAsync();
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var agency = await _context.ShippingAgencies.FindAsync(id);
            if (agency == null) return false;

            _context.ShippingAgencies.Remove(agency);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(List<ShippingAgencyResponseDTO> Items, int TotalCount)> SearchAsync(
            int page,
            int pageSize,
            string? searchTerm,
            string sortBy,
            string sortOrder)
        {
            var query = _context.ShippingAgencies.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                int.TryParse(searchTerm, out var searchNumber);

                query = query.Where(a =>
                    a.Name.Contains(searchTerm) ||
                    (searchNumber > 0 && a.AgencyID == searchNumber)
                );
            }

            var totalCount = await query.CountAsync();

            query = ApplySorting(query, sortBy, sortOrder);

            var agencies = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = agencies.Select(ShippingAgencyResponseDTO.FromShippingAgency).ToList();

            return (items, totalCount);
        }

        private IQueryable<ShippingAgency> ApplySorting(IQueryable<ShippingAgency> query, string sortBy, string sortOrder)
        {
            var isDescending = sortOrder?.ToLower() == "desc";

            return sortBy?.ToLower() switch
            {
                "id" or "agencyid" => isDescending ? query.OrderByDescending(a => a.AgencyID) : query.OrderBy(a => a.AgencyID),
                "name" or "nome"   => isDescending ? query.OrderByDescending(a => a.Name)     : query.OrderBy(a => a.Name),
                _                  => query.OrderBy(a => a.Name)
            };
        }
    }
}
