using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebSafeDockingAPI.Models
{
    [Table("grupos")]
    public class Grupo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("grupoId")]
        public int GrupoId { get; set; }

        [StringLength(40)]
        [Column("grupoNome")]
        public string? GrupoNome { get; set; }

        [StringLength(40)]
        [Column("grupoDescricao")]
        public string? GrupoDescricao { get; set; }

        [Column("criadoEm")]
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        [Column("criadoPorUsuarioId")]
        [ForeignKey("CriadoPorUsuario")]
        public int CriadoPorUsuarioId { get; set; }

        public Usuario? CriadoPorUsuario { get; set; }
    }
}
