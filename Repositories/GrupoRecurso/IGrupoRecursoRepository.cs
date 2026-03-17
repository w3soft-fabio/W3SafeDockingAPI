using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Repositories
{
    public interface IGrupoRecursoRepository
    {
        Task<GrupoRecurso?> GetByIdAsync(int grupoId, string recursoChave);
        Task<GrupoRecurso> CreateAsync(GrupoRecurso grupoRecurso);
        Task<bool> UpdateAsync(GrupoRecurso grupoRecurso);
        Task<bool> DeleteAsync(int grupoId, string recursoChave);

        /// <summary>
        /// Busca vinculos grupo-recurso por termo com paginacao e ordenacao.
        /// </summary>
        Task<(List<GrupoRecursoResponseDTO> Items, int TotalCount)> SearchAsync(
            int page,
            int pageSize,
            string? searchTerm,
            string sortBy,
            string sortOrder);
    }
}
