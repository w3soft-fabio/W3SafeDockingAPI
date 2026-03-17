using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebSafeDockingAPI.Models
{
    [Table("berthings")]
    public class Berthing
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("berthingID")]
        public int BerthingID { get; set; }

        [Required]
        [Column("berth")]
        public int Berth { get; set; }

        [Column("schedule")]
        public int? Schedule { get; set; }

        [Required]
        [Column("shipID")]
        public int ShipID { get; set; }

        [Column("mooringCompanyID")]
        public int? MooringCompanyID { get; set; }

        [Column("agencyID")]
        public int? AgencyID { get; set; }
        // Calado de chegada da proa
        [Column("arrivalDraftFore")]
        public decimal? ArrivalDraftFore { get; set; }
        // Calado de chegada da popa
        [Column("arrivalDraftAft")]
        public decimal? ArrivalDraftAft { get; set; }
        // Calado de saída da proa
        [Column("departureDraftFore")]
        public decimal? DepartureDraftFore { get; set; }
        // Calado de saída da popa    
        [Column("departureDraftAft")]
        public decimal? DepartureDraftAft { get; set; }
 
        //Borda de atracação (ex: "bombordo", "estibordo", "centro")
        [StringLength(20)]
        [Column("side")]
        public string? Side { get; set; }
        
        [Column("arrivalAt")]
        public DateTime? ArrivalAt { get; set; }
        [Column("departureAt")]
        public DateTime? DepartureAt { get; set; }
        // Navigation properties
        [ForeignKey("ShipID")]
        public Ship? Ship { get; set; }

        [ForeignKey("MooringCompanyID")]
        public MooringCompany? MooringCompany { get; set; }

        [ForeignKey("AgencyID")]
        public ShippingAgency? ShippingAgency { get; set; }

    }
}
