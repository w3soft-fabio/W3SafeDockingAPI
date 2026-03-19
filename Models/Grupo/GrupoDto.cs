using System.ComponentModel.DataAnnotations;

namespace WebSafeDockingAPI.Models
{
    /// <summary>
    /// DTO para CREATE e UPDATE de Grupo.
    /// </summary>
    public class GrupoCreateUpdateDTO
    {
        [Required(ErrorMessage = "O nome do grupo e obrigatorio.")]
        [StringLength(40, ErrorMessage = "O nome do grupo deve ter no maximo 40 caracteres.")]
        public string GrupoNome { get; set; } = string.Empty;

        [StringLength(40, ErrorMessage = "A descricao do grupo deve ter no maximo 40 caracteres.")]
        public string? GrupoDescricao { get; set; }

        [Required(ErrorMessage = "O ID do usuario criador e obrigatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "O ID do usuario criador deve ser maior que zero.")]
        public int CriadoPorUsuarioId { get; set; }

        /// <summary>
        /// Converte o DTO para a entidade Grupo.
        /// </summary>
        public Grupo ToEntity()
        {
            return new Grupo
            {
                GrupoNome = this.GrupoNome,
                GrupoDescricao = this.GrupoDescricao,
                CriadoEm = DateTime.UtcNow,
                CriadoPorUsuarioId = this.CriadoPorUsuarioId
            };
        }
    }

    /// <summary>
    /// DTO para RESPONSE de Grupo.
    /// </summary>
    public class GrupoResponseDTO
    {
        public int GrupoId { get; set; }
        public string? GrupoNome { get; set; }
        public string? GrupoDescricao { get; set; }
        public DateTime CriadoEm { get; set; }
        public int CriadoPorUsuarioId { get; set; }

        /// <summary>
        /// Converte a entidade Grupo para GrupoResponseDTO.
        /// </summary>
        public static GrupoResponseDTO FromGrupo(Grupo grupo)
        {
            return new GrupoResponseDTO
            {
                GrupoId = grupo.GrupoId,
                GrupoNome = grupo.GrupoNome,
                GrupoDescricao = grupo.GrupoDescricao,
                CriadoEm = grupo.CriadoEm,
                CriadoPorUsuarioId = grupo.CriadoPorUsuarioId
            };
        }
    }
}
