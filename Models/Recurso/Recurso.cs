using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebSafeDockingAPI.Models
{
    [Table("recursos")]
    public class Recurso
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [StringLength(10)]
        [Column("recursoChave")]
        public string RecursoChave { get; set; } = string.Empty;

        [StringLength(20)]
        [Column("recursoArea")]
        public string? RecursoArea { get; set; }

        [StringLength(200)]
        [Column("recursoDescricao")]
        public string? RecursoDescricao { get; set; }

        [StringLength(200)]
        [Column("descricaoInstrutiva")]
        public string? DescricaoInstrutiva { get; set; }

        [StringLength(200)]
        [Column("urlVideo")]
        public string? UrlVideo { get; set; }
    }
}
