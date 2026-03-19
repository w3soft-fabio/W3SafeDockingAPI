using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Repositories
{
    public interface IEmailContaRepository
    {
        Task<EmailConta?> GetByIdAsync(int id);
        Task<EmailConta> CreateAsync(EmailConta emailConta);
        Task<bool> UpdateAsync(EmailConta emailConta);
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Busca contas de e-mail por termo com paginacao e ordenacao.
        /// </summary>
        Task<(List<EmailContaResponseDTO> Items, int TotalCount)> SearchAsync(
            int page,
            int pageSize,
            string? searchTerm,
            string sortBy,
            string sortOrder);
    }
}
