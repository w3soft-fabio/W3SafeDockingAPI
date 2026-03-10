using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebSafeDockingAPI.Models
{
    /// <summary>
    /// Entidade que representa um Refresh Token no banco de dados.
    /// Usado para renovar o Access Token sem precisar fazer login novamente.
    /// </summary>
    [Table("refresh_tokens")]
    public class RefreshToken
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("usuarioId")]
        public int UsuarioId { get; set; }

        [Required]
        [StringLength(500)]
        [Column("token")]
        public string Token { get; set; } = string.Empty;

        [Column("criadoEm")]
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        [Column("expiraEm")]
        public DateTime ExpiraEm { get; set; }

        [Column("revogado")]
        public bool Revogado { get; set; } = false;

        /// <summary>
        /// Verifica se o token já expirou.
        /// </summary>
        [NotMapped]
        public bool Expirado => DateTime.UtcNow >= ExpiraEm;

        /// <summary>
        /// Um token só é válido se NÃO estiver revogado E NÃO estiver expirado.
        /// </summary>
        [NotMapped]
        public bool Ativo => !Revogado && !Expirado;

        // Relacionamento com Usuario
        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }
    }
}
