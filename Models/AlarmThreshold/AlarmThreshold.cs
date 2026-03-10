using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebSafeDockingAPI.Models
{
    [Table("alarm_threshold")]
    public class AlarmThreshold
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("berthingID")]
        [ForeignKey("BerthingAlarm")]
        public int? BerthingID { get; set; }

        /// <summary>
        /// Propriedade de navegação para BerthingAlarm.
        /// </summary>
        public BerthingAlarm? BerthingAlarm { get; set; }

        [Column("driftingID")]
        [ForeignKey("DriftingAlarm")]
        public int? DriftingID { get; set; }

        /// <summary>
        /// Propriedade de navegação para DriftingAlarm.
        /// </summary>
        public DriftingAlarm? DriftingAlarm { get; set; }

        [Column("minDwt")]
        public int? MinDwt { get; set; }

        [Column("maxDwt")]
        public int? MaxDwt { get; set; }
    }
}
