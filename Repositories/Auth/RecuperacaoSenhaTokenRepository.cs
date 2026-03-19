using Microsoft.EntityFrameworkCore;
using WebSafeDockingAPI.Data;
using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Repositories
{
    /// <summary>
    /// Repositorio para persistencia de tokens de recuperacao de senha.
    /// </summary>
    public class RecuperacaoSenhaTokenRepository : IRecuperacaoSenhaTokenRepository
    {
        private readonly DataDbContext _context;

        public RecuperacaoSenhaTokenRepository(DataDbContext context)
        {
            _context = context;
        }

        public async Task<RecuperacaoSenhaToken> CriarAsync(RecuperacaoSenhaToken token)
        {
            _context.RecuperacaoSenhaTokens.Add(token);
            await _context.SaveChangesAsync();
            return token;
        }

        public async Task<RecuperacaoSenhaToken?> BuscarPorHashAsync(string tokenHash)
        {
            return await _context.RecuperacaoSenhaTokens
                .Include(t => t.Usuario)
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
        }

        public async Task InvalidarTokensAtivosAsync(int usuarioId)
        {
            var ativos = await _context.RecuperacaoSenhaTokens
                .Where(t => t.UsuarioId == usuarioId && !t.Revogado && t.UsadoEm == null && t.ExpiraEm > DateTime.UtcNow)
                .ToListAsync();

            if (ativos.Count == 0) return;

            foreach (var token in ativos)
            {
                token.Revogado = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task MarcarComoUsadoAsync(RecuperacaoSenhaToken token)
        {
            _context.RecuperacaoSenhaTokens.Update(token);
            await _context.SaveChangesAsync();
        }
    }
}
