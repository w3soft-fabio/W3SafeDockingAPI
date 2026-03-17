using System.ComponentModel.DataAnnotations;

namespace WebSafeDockingAPI.Models
{
    /// <summary>
    /// DTO para CREATE e UPDATE de Berthing.
    /// </summary>
    public class BerthingCreateUpdateDTO
    {
        [Required(ErrorMessage = "O campo Berth é obrigatório.")]
        public int Berth { get; set; }

        public int? Schedule { get; set; }

        [Required(ErrorMessage = "O campo ShipID é obrigatório.")]
        public int ShipID { get; set; }

        public int? MooringCompanyID { get; set; }

        public int? AgencyID { get; set; }

        public decimal? ArrivalDraftFore { get; set; }

        public decimal? ArrivalDraftAft { get; set; }

        public decimal? DepartureDraftFore { get; set; }

        public decimal? DepartureDraftAft { get; set; }

        [StringLength(20, ErrorMessage = "O campo Side deve ter no máximo 20 caracteres.")]
        public string? Side { get; set; }

        public DateTime? ArrivalAt { get; set; }

        public DateTime? DepartureAt { get; set; }

        public Berthing ToEntity()
        {
            return new Berthing
            {
                Berth = this.Berth,
                Schedule = this.Schedule,
                ShipID = this.ShipID,
                MooringCompanyID = this.MooringCompanyID,
                AgencyID = this.AgencyID,
                ArrivalDraftFore = this.ArrivalDraftFore,
                ArrivalDraftAft = this.ArrivalDraftAft,
                DepartureDraftFore = this.DepartureDraftFore,
                DepartureDraftAft = this.DepartureDraftAft,
                Side = this.Side,
                ArrivalAt = this.ArrivalAt,
                DepartureAt = this.DepartureAt
            };
        }
    }

    /// <summary>
    /// DTO para RESPONSE de Berthing.
    /// </summary>
    public class BerthingResponseDTO
    {
        public int BerthingID { get; set; }
        public int Berth { get; set; }
        public int? Schedule { get; set; }
        public int ShipID { get; set; }
        public string? ShipName { get; set; }
        public int? MooringCompanyID { get; set; }
        public string? MooringCompanyName { get; set; }
        public int? AgencyID { get; set; }
        public string? AgencyName { get; set; }
        public decimal? ArrivalDraftFore { get; set; }
        public decimal? ArrivalDraftAft { get; set; }
        public decimal? DepartureDraftFore { get; set; }
        public decimal? DepartureDraftAft { get; set; }
        public string? Side { get; set; }
        public DateTime? ArrivalAt { get; set; }
        public DateTime? DepartureAt { get; set; }

        public static BerthingResponseDTO FromBerthing(Berthing berthing)
        {
            return new BerthingResponseDTO
            {
                BerthingID = berthing.BerthingID,
                Berth = berthing.Berth,
                Schedule = berthing.Schedule,
                ShipID = berthing.ShipID,
                ShipName = berthing.Ship?.Name,
                MooringCompanyID = berthing.MooringCompanyID,
                MooringCompanyName = berthing.MooringCompany?.Name,
                AgencyID = berthing.AgencyID,
                AgencyName = berthing.ShippingAgency?.Name,
                ArrivalDraftFore = berthing.ArrivalDraftFore,
                ArrivalDraftAft = berthing.ArrivalDraftAft,
                DepartureDraftFore = berthing.DepartureDraftFore,
                DepartureDraftAft = berthing.DepartureDraftAft,
                Side = berthing.Side,
                ArrivalAt = berthing.ArrivalAt,
                DepartureAt = berthing.DepartureAt
            };
        }
    }
}
