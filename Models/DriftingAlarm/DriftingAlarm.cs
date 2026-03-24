using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebSafeDockingAPI.Models
{
    [Table("drifting_alarm")]
    public class DriftingAlarm
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("maxShipAngle")]
        public decimal? MaxShipAngle { get; set; }

        [Column("outboundWarning")]
        public decimal? OutboundWarning { get; set; }

        [Column("outboundAlarm")]
        public decimal? OutboundAlarm { get; set; }

        [Column("inboundWarning")]
        public decimal? InboundWarning { get; set; }

        [Column("inboundAlarm")]
        public decimal? InboundAlarm { get; set; }
    }
}
