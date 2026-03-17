using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebSafeDockingAPI.Models
{
    [Table("grupos_recursos")]
    [PrimaryKey(nameof(GrupoId), nameof(RecursoChave))]
    public class GrupoRecurso
    {
        [Column("grupoId")]
        [ForeignKey("Grupo")]
        public int GrupoId { get; set; }

        [StringLength(10)]
        [Column("recursoChave")]
        public string RecursoChave { get; set; } = string.Empty;

        public Grupo? Grupo { get; set; }
    }
}
