using System.ComponentModel.DataAnnotations;

namespace WebSafeDockingAPI.Models
{
    /// <summary>
    /// DTO para requisicao de login.
    /// O usuario envia CPF e senha para se autenticar.
    /// </summary>
    public class LoginRequest
    {
        [Required(ErrorMessage = "O CPF e obrigatorio.")]
        [StringLength(14, ErrorMessage = "O CPF deve ter no maximo 14 caracteres.")]
        public string Cpf { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha e obrigatoria.")]
        public string Senha { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO de resposta apos login ou refresh bem-sucedido.
    /// Contem o Access Token (JWT) e o Refresh Token.
    /// </summary>
    public class LoginResponse
    {
        public int UsuarioId { get; set; }

        /// <summary>
        /// Access Token JWT - usado no header Authorization das requisicoes.
        /// Duracao: 1 hora.
        /// </summary>
        public string Cpf { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Refresh Token - usado para renovar o Access Token quando ele expira.
        /// Duracao: 7 dias.
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Data/hora de expiracao do Access Token.
        /// </summary>
        public DateTime Expiracao { get; set; }

        /// <summary>
        /// Nome do usuario autenticado.
        /// </summary>
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Nivel de acesso do usuario (ex: administrador, operador_portuario...).
        /// </summary>
        public string NivelAcesso { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para requisicao de refresh ou revogacao de token.
    /// </summary>
    public class RefreshTokenRequest
    {
        [Required(ErrorMessage = "O Refresh Token e obrigatorio.")]
        public string RefreshToken { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para definir a primeira senha via link de primeiro acesso.
    /// </summary>
    public class DefinirPrimeiraSenhaRequest
    {
        [Required(ErrorMessage = "O token e obrigatorio.")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "A nova senha e obrigatoria.")]
        [MinLength(8, ErrorMessage = "A nova senha deve ter no minimo 8 caracteres.")]
        public string NovaSenha { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para solicitar reenvio do link de primeiro acesso.
    /// </summary>
    public class ReenviarPrimeiroAcessoRequest
    {
        [Required(ErrorMessage = "O CPF e obrigatorio.")]
        [StringLength(14, ErrorMessage = "O CPF deve ter no maximo 14 caracteres.")]
        public string Cpf { get; set; } = string.Empty;
    }
}
