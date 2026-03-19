using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebSafeDockingAPI.Models
{
    /// <summary>
    /// Token de recuperacao de senha (esqueci a senha).
    /// </summary>
    [Table("recuperacao_senha_tokens")]
    public class RecuperacaoSenhaToken
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("usuarioId")]
        public int UsuarioId { get; set; }

        [Required]
        [StringLength(128)]
        [Column("tokenHash")]
        public string TokenHash { get; set; } = string.Empty;

        [Column("criadoEm")]
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        [Column("expiraEm")]
        public DateTime ExpiraEm { get; set; }

        [Column("usadoEm")]
        public DateTime? UsadoEm { get; set; }

        [Column("revogado")]
        public bool Revogado { get; set; } = false;

        [NotMapped]
        public bool Expirado => DateTime.UtcNow >= ExpiraEm;

        [NotMapped]
        public bool Ativo => !Revogado && !Expirado && UsadoEm == null;

        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }
    }
}
