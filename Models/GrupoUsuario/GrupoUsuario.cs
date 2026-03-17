using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebSafeDockingAPI.Models
{
    [Table("grupos_usuarios")]
    [PrimaryKey(nameof(GrupoId), nameof(UsuarioId))]
    public class GrupoUsuario
    {
        [Column("grupoId")]
        [ForeignKey("Grupo")]
        public int GrupoId { get; set; }

        [Column("usuarioId")]
        [ForeignKey("Usuario")]
        public int UsuarioId { get; set; }

        public Grupo? Grupo { get; set; }
        public Usuario? Usuario { get; set; }
    }
}
