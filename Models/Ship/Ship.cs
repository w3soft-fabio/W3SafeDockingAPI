using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebSafeDockingAPI.Models
{
    [Table("ship")]
    public class Ship
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("imo")]
        public int Imo { get; set; }

        [StringLength(255)]
        [Column("name")]
        public string? Name { get; set; }

        [Column("length")]
        public int? Length { get; set; }

        [Column("width")]
        public int? Width { get; set; }

        [Column("dwt")]
        public int? Dwt { get; set; }

        [Column("alarmId")]
        public int? AlarmID { get; set; }
    }
}
