using System.ComponentModel.DataAnnotations;

namespace WebSafeDockingAPI.Models
{
    /// <summary>
    /// DTO para CREATE e UPDATE de MooringCompany.
    /// </summary>
    public class MooringCompanyCreateUpdateDTO
    {
        [Required(ErrorMessage = "O campo Nome é obrigatório.")]
        [StringLength(150, ErrorMessage = "O nome deve ter no máximo 150 caracteres.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Converte o DTO para a entidade MooringCompany.
        /// </summary>
        public MooringCompany ToEntity()
        {
            return new MooringCompany
            {
                Name = this.Name
            };
        }
    }

    /// <summary>
    /// DTO para RESPONSE de MooringCompany.
    /// </summary>
    public class MooringCompanyResponseDTO
    {
        public int MooringCompanyID { get; set; }
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Converte a entidade MooringCompany para MooringCompanyResponseDTO.
        /// </summary>
        public static MooringCompanyResponseDTO FromMooringCompany(MooringCompany mooringCompany)
        {
            return new MooringCompanyResponseDTO
            {
                MooringCompanyID = mooringCompany.MooringCompanyID,
                Name = mooringCompany.Name
            };
        }
    }
}
