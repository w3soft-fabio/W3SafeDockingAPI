using System.ComponentModel.DataAnnotations;

namespace WebSafeDockingAPI.Models
{
    /// <summary>
    /// DTO para CREATE e UPDATE de Recurso.
    /// </summary>
    public class RecursoCreateUpdateDTO
    {
        [Required(ErrorMessage = "A chave do recurso e obrigatoria.")]
        [StringLength(10, ErrorMessage = "A chave do recurso deve ter no maximo 10 caracteres.")]
        public string RecursoChave { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "A area do recurso deve ter no maximo 20 caracteres.")]
        public string? RecursoArea { get; set; }

        [StringLength(200, ErrorMessage = "A descricao do recurso deve ter no maximo 200 caracteres.")]
        public string? RecursoDescricao { get; set; }

        [StringLength(200, ErrorMessage = "A descricao instrutiva deve ter no maximo 200 caracteres.")]
        public string? DescricaoInstrutiva { get; set; }

        [StringLength(200, ErrorMessage = "A URL do video deve ter no maximo 200 caracteres.")]
        public string? UrlVideo { get; set; }

        /// <summary>
        /// Converte o DTO para a entidade Recurso.
        /// </summary>
        public Recurso ToEntity()
        {
            return new Recurso
            {
                RecursoChave = this.RecursoChave.Trim(),
                RecursoArea = this.RecursoArea,
                RecursoDescricao = this.RecursoDescricao,
                DescricaoInstrutiva = this.DescricaoInstrutiva,
                UrlVideo = this.UrlVideo
            };
        }
    }

    /// <summary>
    /// DTO para RESPONSE de Recurso.
    /// </summary>
    public class RecursoResponseDTO
    {
        public string RecursoChave { get; set; } = string.Empty;
        public string? RecursoArea { get; set; }
        public string? RecursoDescricao { get; set; }
        public string? DescricaoInstrutiva { get; set; }
        public string? UrlVideo { get; set; }

        /// <summary>
        /// Converte a entidade Recurso para RecursoResponseDTO.
        /// </summary>
        public static RecursoResponseDTO FromRecurso(Recurso recurso)
        {
            return new RecursoResponseDTO
            {
                RecursoChave = recurso.RecursoChave,
                RecursoArea = recurso.RecursoArea,
                RecursoDescricao = recurso.RecursoDescricao,
                DescricaoInstrutiva = recurso.DescricaoInstrutiva,
                UrlVideo = recurso.UrlVideo
            };
        }
    }
}
