using Microsoft.EntityFrameworkCore;
using WebSafeDockingAPI.Data;
using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Repositories
{
    /// <summary>
    /// Repositório que acessa o banco de dados para operações de autenticação.
    /// Gerencia refresh tokens e busca de usuários para login.
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
        /// Busca um refresh token pelo valor do token (string).
        /// Inclui os dados do usuário associado.
        /// </summary>
        public async Task<RefreshToken?> BuscarPorTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.Usuario)
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        /// <summary>
        /// Atualiza um refresh token existente (ex: marcar como revogado).
        /// </summary>
        public async Task AtualizarAsync(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Update(refreshToken);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Busca um usuário pelo CPF para autenticação no login.
        /// </summary>
        public async Task<Usuario?> BuscarUsuarioPorCpfAsync(string cpf)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Cpf == cpf);
        }

        /// <summary>
        /// Busca um usuário pelo ID (usado ao renovar o token).
        /// </summary>
        public async Task<Usuario?> BuscarUsuarioPorIdAsync(int id)
        {
            return await _context.Usuarios.FindAsync(id);
        }
    }
}
