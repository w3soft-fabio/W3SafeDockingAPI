using System.ComponentModel.DataAnnotations;

namespace WebSafeDockingAPI.Models
{
    /// <summary>
    /// DTO para CREATE e UPDATE de EmailConta.
    /// </summary>
    public class EmailContaCreateUpdateDTO
    {
        [StringLength(100, ErrorMessage = "O apelido deve ter no maximo 100 caracteres.")]
        public string? Apelido { get; set; }

        [StringLength(255, ErrorMessage = "O endereco deve ter no maximo 255 caracteres.")]
        public string? Endereco { get; set; }

        [StringLength(200, ErrorMessage = "O usuario deve ter no maximo 200 caracteres.")]
        public string? CredenciaisUsuario { get; set; }

        [StringLength(200, ErrorMessage = "A senha deve ter no maximo 200 caracteres.")]
        public string? CredenciaisSenha { get; set; }

        [StringLength(200, ErrorMessage = "O host SMTP deve ter no maximo 200 caracteres.")]
        public string? SmtpHost { get; set; }

        [StringLength(10, ErrorMessage = "A porta SMTP deve ter no maximo 10 caracteres.")]
        public string? SmtpPort { get; set; }

        [RegularExpression("^(True|False)$", ErrorMessage = "smtpSSL deve ser 'True' ou 'False'.")]
        public string? SmtpSSL { get; set; }

        /// <summary>
        /// Converte o DTO para a entidade EmailConta.
        /// </summary>
        public EmailConta ToEntity()
        {
            return new EmailConta
            {
                Apelido = this.Apelido,
                Endereco = this.Endereco,
                CredenciaisUsuario = this.CredenciaisUsuario,
                CredenciaisSenha = this.CredenciaisSenha,
                SmtpHost = this.SmtpHost,
                SmtpPort = this.SmtpPort,
                SmtpSSL = this.SmtpSSL
            };
        }
    }

    /// <summary>
    /// DTO para RESPONSE de EmailConta.
    /// </summary>
    public class EmailContaResponseDTO
    {
        public int ContaID { get; set; }
        public string? Apelido { get; set; }
        public string? Endereco { get; set; }
        public string? CredenciaisUsuario { get; set; }
        public string? CredenciaisSenha { get; set; }
        public string? SmtpHost { get; set; }
        public string? SmtpPort { get; set; }
        public string? SmtpSSL { get; set; }

        /// <summary>
        /// Converte a entidade EmailConta para EmailContaResponseDTO.
        /// </summary>
        public static EmailContaResponseDTO FromEmailConta(EmailConta emailConta)
        {
            return new EmailContaResponseDTO
            {
                ContaID = emailConta.ContaID,
                Apelido = emailConta.Apelido,
                Endereco = emailConta.Endereco,
                CredenciaisUsuario = emailConta.CredenciaisUsuario,
                CredenciaisSenha = emailConta.CredenciaisSenha,
                SmtpHost = emailConta.SmtpHost,
                SmtpPort = emailConta.SmtpPort,
                SmtpSSL = emailConta.SmtpSSL
            };
        }
    }
}
