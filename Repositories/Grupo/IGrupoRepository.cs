using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Repositories
{
    public interface IGrupoRepository
    {
        Task<Grupo?> GetByIdAsync(int id);
        Task<Grupo> CreateAsync(Grupo grupo);
        Task<bool> UpdateAsync(Grupo grupo);
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Busca grupos por termo com paginacao e ordenacao.
        /// </summary>
        Task<(List<GrupoResponseDTO> Items, int TotalCount)> SearchAsync(
            int page,
            int pageSize,
            string? searchTerm,
            string sortBy,
            string sortOrder);
    }
}
