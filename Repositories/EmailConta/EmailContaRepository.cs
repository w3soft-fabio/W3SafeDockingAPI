using Microsoft.EntityFrameworkCore;
using WebSafeDockingAPI.Data;
using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Repositories
{
    public class EmailContaRepository : IEmailContaRepository
    {
        private readonly DataDbContext _context;

        public EmailContaRepository(DataDbContext context)
        {
            _context = context;
        }

        public async Task<EmailConta?> GetByIdAsync(int id)
        {
            return await _context.EmailContas.FindAsync(id);
        }

        public async Task<EmailConta?> GetDefaultAsync()
        {
            return await _context.EmailContas
                .OrderBy(ec => ec.ContaID)
                .FirstOrDefaultAsync();
        }

        public async Task<EmailConta> CreateAsync(EmailConta emailConta)
        {
            _context.EmailContas.Add(emailConta);
            await _context.SaveChangesAsync();
            return emailConta;
        }

        public async Task<bool> UpdateAsync(EmailConta emailConta)
        {
            _context.EmailContas.Update(emailConta);
            var rows = await _context.SaveChangesAsync();
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var emailConta = await _context.EmailContas.FindAsync(id);
            if (emailConta == null) return false;

            _context.EmailContas.Remove(emailConta);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Busca contas de e-mail por termo com paginacao e ordenacao.
        /// </summary>
        public async Task<(List<EmailContaResponseDTO> Items, int TotalCount)> SearchAsync(
            int page,
            int pageSize,
            string? searchTerm,
            string sortBy,
            string sortOrder)
        {
            var query = _context.EmailContas.AsQueryable();

            // Aplicar filtro de busca
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                int.TryParse(searchTerm, out var searchNumber);

                query = query.Where(ec =>
                    (ec.Apelido != null && ec.Apelido.Contains(searchTerm)) ||
                    (ec.Endereco != null && ec.Endereco.Contains(searchTerm)) ||
                    (ec.CredenciaisUsuario != null && ec.CredenciaisUsuario.Contains(searchTerm)) ||
                    (ec.SmtpHost != null && ec.SmtpHost.Contains(searchTerm)) ||
                    (searchNumber > 0 && ec.ContaID == searchNumber)
                );
            }

            // Obter total antes da paginacao
            var totalCount = await query.CountAsync();

            // Aplicar ordenacao dinamica
            query = ApplySorting(query, sortBy, sortOrder);

            // Aplicar paginacao e converter para DTO
            var emailContas = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = emailContas.Select(EmailContaResponseDTO.FromEmailConta).ToList();
            return (items, totalCount);
        }

        /// <summary>
        /// Aplica ordenacao dinamica na query.
        /// </summary>
        private IQueryable<EmailConta> ApplySorting(IQueryable<EmailConta> query, string sortBy, string sortOrder)
        {
            var isDescending = sortOrder?.ToLower() == "desc";

            return sortBy?.ToLower() switch
            {
                "id" or "contaid" => isDescending
                    ? query.OrderByDescending(ec => ec.ContaID)
                    : query.OrderBy(ec => ec.ContaID),
                "emailcontaid" => isDescending
                    ? query.OrderByDescending(ec => ec.ContaID)
                    : query.OrderBy(ec => ec.ContaID),
                "apelido" or "nickname" => isDescending
                    ? query.OrderByDescending(ec => ec.Apelido)
                    : query.OrderBy(ec => ec.Apelido),
                "endereco" or "address" => isDescending
                    ? query.OrderByDescending(ec => ec.Endereco)
                    : query.OrderBy(ec => ec.Endereco),
                "credenciaisusuario" or "usuario" => isDescending
                    ? query.OrderByDescending(ec => ec.CredenciaisUsuario)
                    : query.OrderBy(ec => ec.CredenciaisUsuario),
                "smtphost" or "host" => isDescending
                    ? query.OrderByDescending(ec => ec.SmtpHost)
                    : query.OrderBy(ec => ec.SmtpHost),
                "smtpport" or "port" => isDescending
                    ? query.OrderByDescending(ec => ec.SmtpPort)
                    : query.OrderBy(ec => ec.SmtpPort),
                "smtpssl" or "ssl" => isDescending
                    ? query.OrderByDescending(ec => ec.SmtpSSL)
                    : query.OrderBy(ec => ec.SmtpSSL),
                _ => query.OrderBy(ec => ec.Apelido)
            };
        }
    }
}
