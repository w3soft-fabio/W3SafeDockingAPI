using System.ComponentModel.DataAnnotations;

namespace WebSafeDockingAPI.Models
{
    /// <summary>
    /// DTO para requisição de login.
    /// O usuário envia CPF e senha para se autenticar.
    /// </summary>
    public class LoginRequest
    {
        [Required(ErrorMessage = "O CPF é obrigatório.")]
        [StringLength(14, ErrorMessage = "O CPF deve ter no máximo 14 caracteres.")]
        public string Cpf { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória.")]
        public string Senha { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO de resposta após login ou refresh bem-sucedido.
    /// Contém o Access Token (JWT) e o Refresh Token.
    /// </summary>
    public class LoginResponse
    {
        public int UsuarioId { get; set; }
        /// <summary>
        /// Access Token JWT - usado no header Authorization das requisições.
        /// Duração: 1 hora.
        /// </summary>
        /// 
        public string Cpf { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Refresh Token - usado para renovar o Access Token quando ele expira.
        /// Duração: 7 dias.
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Data/hora de expiração do Access Token.
        /// </summary>
        public DateTime Expiracao { get; set; }

        /// <summary>
        /// Nome do usuário autenticado.
        /// </summary>
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Nível de acesso do usuário (ex: administrador, operador_portuario...).
        /// </summary>
        public string NivelAcesso { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para requisição de refresh ou revogação de token.
    /// </summary>
    public class RefreshTokenRequest
    {
        [Required(ErrorMessage = "O Refresh Token é obrigatório.")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
