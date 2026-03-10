namespace WebSafeDockingAPI.Services
{
    /// <summary>
    /// Serviço responsável por criar hash de senhas e verificar senhas.
    /// Usa BCrypt, que é um algoritmo seguro e resistente a ataques de força bruta.
    /// </summary>
    public class PasswordHasherService
    {
        /// <summary>
        /// Cria um hash seguro da senha usando BCrypt.
        /// Cada vez que é chamado, gera um hash diferente (por causa do salt automático).
        /// </summary>
        public string HashPassword(string senha)
        {
            return BCrypt.Net.BCrypt.HashPassword(senha);
        }

        /// <summary>
        /// Verifica se uma senha corresponde ao hash armazenado.
        /// Retorna true se a senha está correta.
        /// </summary>
        public bool VerificarSenha(string senha, string hashArmazenado)
        {
            return BCrypt.Net.BCrypt.Verify(senha, hashArmazenado);
        }
    }
}
