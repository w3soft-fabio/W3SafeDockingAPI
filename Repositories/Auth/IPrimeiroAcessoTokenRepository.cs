using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Repositories
{
    /// <summary>
    /// Repositorio para operacoes de token de primeiro acesso.
    /// </summary>
    public interface IPrimeiroAcessoTokenRepository
    {
        Task<PrimeiroAcessoToken> CriarAsync(PrimeiroAcessoToken token);
        Task<PrimeiroAcessoToken?> BuscarPorHashAsync(string tokenHash);
        Task InvalidarTokensAtivosAsync(int usuarioId);
        Task MarcarComoUsadoAsync(PrimeiroAcessoToken token);
    }
}
