using Microsoft.EntityFrameworkCore;
using WebSafeDockingAPI.Data;
using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Repositories
{
    public class BerthingRepository : IBerthingRepository
    {
        private readonly DataDbContext _context;

        public BerthingRepository(DataDbContext context)
        {
            _context = context;
        }

        public async Task<Berthing?> GetByIdAsync(int id)
        {
            return await _context.Berthings
                .Include(b => b.Ship)
                .Include(b => b.MooringCompany)
                .Include(b => b.ShippingAgency)
                .FirstOrDefaultAsync(b => b.BerthingID == id);
        }

        public async Task<Berthing> CreateAsync(Berthing berthing)
        {
            _context.Berthings.Add(berthing);
            await _context.SaveChangesAsync();

            return await _context.Berthings
                .Include(b => b.Ship)
                .Include(b => b.MooringCompany)
                .Include(b => b.ShippingAgency)
                .FirstAsync(b => b.BerthingID == berthing.BerthingID);
        }

        public async Task<bool> UpdateAsync(Berthing berthing)
        {
            _context.Berthings.Update(berthing);
            var rows = await _context.SaveChangesAsync();
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var berthing = await _context.Berthings.FindAsync(id);
            if (berthing == null) return false;

            _context.Berthings.Remove(berthing);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(List<BerthingResponseDTO> Items, int TotalCount)> SearchAsync(
            int page,
            int pageSize,
            string? searchTerm,
            string sortBy,
            string sortOrder)
        {
            var query = _context.Berthings
                .Include(b => b.Ship)
                .Include(b => b.MooringCompany)
                .Include(b => b.ShippingAgency)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                int.TryParse(searchTerm, out var searchNumber);

                query = query.Where(b =>
                    (b.Ship != null && b.Ship.Name != null && b.Ship.Name.Contains(searchTerm)) ||
                    (b.MooringCompany != null && b.MooringCompany.Name.Contains(searchTerm)) ||
                    (b.ShippingAgency != null && b.ShippingAgency.Name.Contains(searchTerm)) ||
                    (b.Side != null && b.Side.Contains(searchTerm)) ||
                    (searchNumber > 0 && (b.BerthingID == searchNumber || b.Berth == searchNumber || b.ShipID == searchNumber))
                );
            }

            var totalCount = await query.CountAsync();

            query = ApplySorting(query  , sortBy, sortOrder);

            var berthings = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = berthings.Select(BerthingResponseDTO.FromBerthing).ToList();

            return (items, totalCount);
        }

        private IQueryable<Berthing> ApplySorting(IQueryable<Berthing> query, string sortBy, string sortOrder)
        {
            var isDescending = sortOrder?.ToLower() == "desc";

            return sortBy?.ToLower() switch
            {
                "id" or "berthingid"       => isDescending ? query.OrderByDescending(b => b.BerthingID) : query.OrderBy(b => b.BerthingID),
                "berth"                    => isDescending ? query.OrderByDescending(b => b.Berth) : query.OrderBy(b => b.Berth),
                "ship" or "shipname"       => isDescending ? query.OrderByDescending(b => b.Ship!.Name) : query.OrderBy(b => b.Ship!.Name),
                "departureat"              => isDescending ? query.OrderByDescending(b => b.DepartureAt) : query.OrderBy(b => b.DepartureAt),
                "side"                     => isDescending ? query.OrderByDescending(b => b.Side) : query.OrderBy(b => b.Side),
                _                          => query.OrderBy(b => b.BerthingID)
            };
        }
    }
}
