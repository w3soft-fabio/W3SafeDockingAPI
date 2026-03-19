using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Repositories
{
    /// <summary>
    /// Repositorio para operacoes de token de recuperacao de senha.
    /// </summary>
    public interface IRecuperacaoSenhaTokenRepository
    {
        Task<RecuperacaoSenhaToken> CriarAsync(RecuperacaoSenhaToken token);
        Task<RecuperacaoSenhaToken?> BuscarPorHashAsync(string tokenHash);
        Task InvalidarTokensAtivosAsync(int usuarioId);
        Task MarcarComoUsadoAsync(RecuperacaoSenhaToken token);
    }
}
