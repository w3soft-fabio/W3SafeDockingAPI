using Microsoft.EntityFrameworkCore;
using WebSafeDockingAPI.Data;
using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Repositories
{
    /// <summary>
    /// Repositorio que acessa o banco para operacoes de autenticacao.
    /// </summary>
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly DataDbContext _context;

        public RefreshTokenRepository(DataDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Salva um novo refresh token no banco de dados.
        /// </summary>
        public async Task<RefreshToken> CriarAsync(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
            return refreshToken;
        }

        /// <summary>
        /// Busca um refresh token pelo valor do token.
        /// Inclui os dados do usuario associado.
        /// </summary>
        public async Task<RefreshToken?> BuscarPorTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.Usuario)
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        /// <summary>
        /// Atualiza um refresh token existente.
        /// </summary>
        public async Task AtualizarAsync(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Update(refreshToken);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Busca um usuario pelo CPF para autenticacao no login.
        /// </summary>
        public async Task<Usuario?> BuscarUsuarioPorCpfAsync(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return null;

            var cpfNormalizado = new string(cpf.Where(char.IsDigit).ToArray());

            return await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.Cpf != null &&
                    (
                        u.Cpf == cpf ||
                        u.Cpf == cpfNormalizado ||
                        u.Cpf.Replace(".", "").Replace("-", "") == cpfNormalizado
                    ));
        }

        /// <summary>
        /// Busca um usuario pelo ID (usado ao renovar o token).
        /// </summary>
        public async Task<Usuario?> BuscarUsuarioPorIdAsync(int id)
        {
            return await _context.Usuarios.FindAsync(id);
        }

        /// <summary>
        /// Revoga todos os refresh tokens ativos de um usuario.
        /// </summary>
        public async Task RevogarTodosPorUsuarioAsync(int usuarioId)
        {
            var tokensAtivos = await _context.RefreshTokens
                .Where(rt => rt.UsuarioId == usuarioId && !rt.Revogado && rt.ExpiraEm > DateTime.UtcNow)
                .ToListAsync();

            if (tokensAtivos.Count == 0) return;

            foreach (var token in tokensAtivos)
            {
                token.Revogado = true;
            }

            await _context.SaveChangesAsync();
        }
    }
}
