using System.ComponentModel.DataAnnotations;

namespace WebSafeDockingAPI.Models
{
    /// <summary>
    /// DTO para CREATE e UPDATE de ShippingAgency.
    /// </summary>
    public class ShippingAgencyCreateUpdateDTO
    {
        [Required(ErrorMessage = "O campo Nome é obrigatório.")]
        [StringLength(150, ErrorMessage = "O nome deve ter no máximo 150 caracteres.")]
        public string Name { get; set; } = string.Empty;

        public ShippingAgency ToEntity()
        {
            return new ShippingAgency
            {
                Name = this.Name
            };
        }
    }

    /// <summary>
    /// DTO para RESPONSE de ShippingAgency.
    /// </summary>
    public class ShippingAgencyResponseDTO
    {
        public int AgencyID { get; set; }
        public string Name { get; set; } = string.Empty;

        public static ShippingAgencyResponseDTO FromShippingAgency(ShippingAgency agency)
        {
            return new ShippingAgencyResponseDTO
            {
                AgencyID = agency.AgencyID,
                Name = agency.Name
            };
        }
    }
}
