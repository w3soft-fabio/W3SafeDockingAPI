using Microsoft.EntityFrameworkCore;
using WebSafeDockingAPI.Data;
using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Repositories
{
    /// <summary>
    /// Repositorio para persistencia de tokens de primeiro acesso.
    /// </summary>
    public class PrimeiroAcessoTokenRepository : IPrimeiroAcessoTokenRepository
    {
        private readonly DataDbContext _context;

        public PrimeiroAcessoTokenRepository(DataDbContext context)
        {
            _context = context;
        }

        public async Task<PrimeiroAcessoToken> CriarAsync(PrimeiroAcessoToken token)
        {
            _context.PrimeiroAcessoTokens.Add(token);
            await _context.SaveChangesAsync();
            return token;
        }

        public async Task<PrimeiroAcessoToken?> BuscarPorHashAsync(string tokenHash)
        {
            return await _context.PrimeiroAcessoTokens
                .Include(t => t.Usuario)
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
        }

        public async Task InvalidarTokensAtivosAsync(int usuarioId)
        {
            var ativos = await _context.PrimeiroAcessoTokens
                .Where(t => t.UsuarioId == usuarioId && !t.Revogado && t.UsadoEm == null && t.ExpiraEm > DateTime.UtcNow)
                .ToListAsync();

            if (ativos.Count == 0) return;

            foreach (var token in ativos)
            {
                token.Revogado = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task MarcarComoUsadoAsync(PrimeiroAcessoToken token)
        {
            _context.PrimeiroAcessoTokens.Update(token);
            await _context.SaveChangesAsync();
        }
    }
}
