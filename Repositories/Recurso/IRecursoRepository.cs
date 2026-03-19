using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Repositories
{
    public interface IRecursoRepository
    {
        Task<Recurso?> GetByIdAsync(string recursoChave);
        Task<Recurso> CreateAsync(Recurso recurso);
        Task<bool> UpdateAsync(Recurso recurso);
        Task<bool> DeleteAsync(string recursoChave);

        /// <summary>
        /// Busca recursos por termo com paginacao e ordenacao.
        /// </summary>
        Task<(List<RecursoResponseDTO> Items, int TotalCount)> SearchAsync(
            int page,
            int pageSize,
            string? searchTerm,
            string sortBy,
            string sortOrder);
    }
}
