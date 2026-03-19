using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Repositories
{
    /// <summary>
    /// Interface do repositorio de Refresh Tokens.
    /// Define operacoes de acesso ao banco para gerenciar refresh tokens.
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
        /// Busca um usuario pelo CPF (usado no login).
        /// </summary>
        Task<Usuario?> BuscarUsuarioPorCpfAsync(string cpf);

        /// <summary>
        /// Busca um usuario pelo ID (usado no refresh).
        /// </summary>
        Task<Usuario?> BuscarUsuarioPorIdAsync(int id);

        /// <summary>
        /// Revoga todos os refresh tokens ativos de um usuario.
        /// </summary>
        Task RevogarTodosPorUsuarioAsync(int usuarioId);
    }
}
