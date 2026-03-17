using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Repositories
{
    public interface IGrupoUsuarioRepository
    {
        Task<GrupoUsuario?> GetByIdAsync(int grupoId, int usuarioId);
        Task<GrupoUsuario> CreateAsync(GrupoUsuario grupoUsuario);
        Task<bool> UpdateAsync(GrupoUsuario grupoUsuario);
        Task<bool> DeleteAsync(int grupoId, int usuarioId);

        /// <summary>
        /// Busca vinculos grupo-usuario por termo com paginacao e ordenacao.
        /// </summary>
        Task<(List<GrupoUsuarioResponseDTO> Items, int TotalCount)> SearchAsync(
            int page,
            int pageSize,
            string? searchTerm,
            string sortBy,
            string sortOrder);
    }
}
