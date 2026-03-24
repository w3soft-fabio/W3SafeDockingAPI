using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebSafeDockingAPI.Models
{
    [Table("berthing_alarm")]
    public class BerthingAlarm
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("maxShipAngle")]
        public decimal? MaxShipAngle { get; set; }

        [Column("zcMinRange")]
        public decimal? ZcMinRange { get; set; }

        [Column("zcMaxRange")]
        public decimal? ZcMaxRange { get; set; }

        [Column("zcSpeedWarning")]
        public int? ZcSpeedWarning { get; set; }

        [Column("zcSpeedAlarm")]
        public int? ZcSpeedAlarm { get; set; }

        [Column("zmMinRange")]
        public decimal? ZmMinRange { get; set; }

        [Column("zmMaxRange")]
        public decimal? ZmMaxRange { get; set; }

        [Column("zmSpeedWarning")]
        public int? ZmSpeedWarning { get; set; }

        [Column("zmSpeedAlarm")]
        public int? ZmSpeedAlarm { get; set; }

        [Column("zfMinRange")]
        public decimal? ZfMinRange { get; set; }

        [Column("zfMaxRange")]
        public decimal? ZfMaxRange { get; set; }

        [Column("zfSpeedWarning")]
        public int? ZfSpeedWarning { get; set; }

        [Column("zfSpeedAlarm")]
        public int? ZfSpeedAlarm { get; set; }
    }
}
