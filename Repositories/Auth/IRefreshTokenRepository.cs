using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Repositories
{
    /// <summary>
    /// Interface do repositório de Refresh Tokens.
    /// Define as operações de acesso ao banco de dados para gerenciar refresh tokens.
    /// </summary>
    public interface IRefreshTokenRepository
    {
        /// <summary>
        /// Cria um novo refresh token no banco de dados.
        /// </summary>
        Task<RefreshToken> CriarAsync(RefreshToken refreshToken);

        /// <summary>
        /// Busca um refresh token pelo valor do token.
        /// </summary>
        Task<RefreshToken?> BuscarPorTokenAsync(string token);

        /// <summary>
        /// Atualiza um refresh token (usado para revogar).
        /// </summary>
        Task AtualizarAsync(RefreshToken refreshToken);

        /// <summary>
        /// Busca um usuário pelo CPF (usado no login).
        /// </summary>
        Task<Usuario?> BuscarUsuarioPorCpfAsync(string cpf);

        /// <summary>
        /// Busca um usuário pelo ID (usado no refresh).
        /// </summary>
        Task<Usuario?> BuscarUsuarioPorIdAsync(int id);
    }
}
