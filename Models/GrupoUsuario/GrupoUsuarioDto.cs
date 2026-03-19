using System.ComponentModel.DataAnnotations;

namespace WebSafeDockingAPI.Models
{
    /// <summary>
    /// DTO para CREATE e UPDATE de GrupoUsuario.
    /// </summary>
    public class GrupoUsuarioCreateUpdateDTO
    {
        [Required(ErrorMessage = "O ID do grupo e obrigatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "O ID do grupo deve ser maior que zero.")]
        public int GrupoId { get; set; }

        [Required(ErrorMessage = "O ID do usuario e obrigatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "O ID do usuario deve ser maior que zero.")]
        public int UsuarioId { get; set; }

        /// <summary>
        /// Converte o DTO para a entidade GrupoUsuario.
        /// </summary>
        public GrupoUsuario ToEntity()
        {
            return new GrupoUsuario
            {
                GrupoId = this.GrupoId,
                UsuarioId = this.UsuarioId
            };
        }
    }

    /// <summary>
    /// DTO para RESPONSE de GrupoUsuario.
    /// </summary>
    public class GrupoUsuarioResponseDTO
    {
        public int GrupoId { get; set; }
        public int UsuarioId { get; set; }

        /// <summary>
        /// Converte a entidade GrupoUsuario para GrupoUsuarioResponseDTO.
        /// </summary>
        public static GrupoUsuarioResponseDTO FromGrupoUsuario(GrupoUsuario grupoUsuario)
        {
            return new GrupoUsuarioResponseDTO
            {
                GrupoId = grupoUsuario.GrupoId,
                UsuarioId = grupoUsuario.UsuarioId
            };
        }
    }
}
