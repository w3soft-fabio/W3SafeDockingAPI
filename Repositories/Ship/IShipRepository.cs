using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Repositories
{
    public interface IShipRepository
    {
        Task<Ship?> GetByIdAsync(int id);
        Task<Ship> CreateAsync(Ship ship);
        Task<bool> UpdateAsync(Ship ship);
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Busca navios por termo com paginação e ordenação.
        /// </summary>
        Task<(List<ShipResponseDTO> Items, int TotalCount)> SearchAsync(
            int page,
            int pageSize,
            string? searchTerm,
            string sortBy,
            string sortOrder);
    }
}
