using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebSafeDockingAPI.Models
{
    /// <summary>
    /// Entidade de conta de e-mail SMTP.
    /// </summary>
    [Table("e_mail_contas")]
    public class EmailConta
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("contaID")]
        public int ContaID { get; set; }

        [StringLength(100)]
        [Column("apelido")]
        public string? Apelido { get; set; }

        [StringLength(255)]
        [Column("endereco")]
        public string? Endereco { get; set; }

        [StringLength(200)]
        [Column("credenciaisUsuario")]
        public string? CredenciaisUsuario { get; set; }

        [StringLength(200)]
        [Column("credenciaisSenha")]
        public string? CredenciaisSenha { get; set; }

        [StringLength(200)]
        [Column("smtpHost")]
        public string? SmtpHost { get; set; }

        [StringLength(10)]
        [Column("smtpPort")]
        public string? SmtpPort { get; set; }

        [StringLength(5)]
        [Column("smtpSSL")]
        public string? SmtpSSL { get; set; }
    }
}
