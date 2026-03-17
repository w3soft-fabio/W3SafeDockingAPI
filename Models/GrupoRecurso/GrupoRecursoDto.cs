using System.ComponentModel.DataAnnotations;

namespace WebSafeDockingAPI.Models
{
    /// <summary>
    /// DTO para CREATE e UPDATE de GrupoRecurso.
    /// </summary>
    public class GrupoRecursoCreateUpdateDTO
    {
        [Required(ErrorMessage = "O ID do grupo e obrigatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "O ID do grupo deve ser maior que zero.")]
        public int GrupoId { get; set; }

        [Required(ErrorMessage = "A chave do recurso e obrigatoria.")]
        [StringLength(10, ErrorMessage = "A chave do recurso deve ter no maximo 10 caracteres.")]
        public string RecursoChave { get; set; } = string.Empty;

        /// <summary>
        /// Converte o DTO para a entidade GrupoRecurso.
        /// </summary>
        public GrupoRecurso ToEntity()
        {
            return new GrupoRecurso
            {
                GrupoId = this.GrupoId,
                RecursoChave = this.RecursoChave.Trim()
            };
        }
    }

    /// <summary>
    /// DTO para RESPONSE de GrupoRecurso.
    /// </summary>
    public class GrupoRecursoResponseDTO
    {
        public int GrupoId { get; set; }
        public string RecursoChave { get; set; } = string.Empty;

        /// <summary>
        /// Converte a entidade GrupoRecurso para GrupoRecursoResponseDTO.
        /// </summary>
        public static GrupoRecursoResponseDTO FromGrupoRecurso(GrupoRecurso grupoRecurso)
        {
            return new GrupoRecursoResponseDTO
            {
                GrupoId = grupoRecurso.GrupoId,
                RecursoChave = grupoRecurso.RecursoChave
            };
        }
    }
}
