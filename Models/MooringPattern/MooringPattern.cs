using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebSafeDockingAPI.Models
{
    [Table("mooring_pattern")]
    public class MooringPattern
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [StringLength(255)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("jettyID")]
        public int? JettyID { get; set; }

        // Grupo QRH 1
        [Column("qrh1H1")]
        public int? Qrh1H1 { get; set; }
        [Column("qrh1H2")]
        public int? Qrh1H2 { get; set; }
        [Column("qrh1H3")]
        public int? Qrh1H3 { get; set; }
        [Column("qrh1H4")]
        public int? Qrh1H4 { get; set; }

        // Grupo QRH 2
        [Column("qrh2H1")]
        public int? Qrh2H1 { get; set; }
        [Column("qrh2H2")]
        public int? Qrh2H2 { get; set; }
        [Column("qrh2H3")]
        public int? Qrh2H3 { get; set; }
        [Column("qrh2H4")]
        public int? Qrh2H4 { get; set; }

        // Grupo QRH 3
        [Column("qrh3H1")]
        public int? Qrh3H1 { get; set; }
        [Column("qrh3H2")]
        public int? Qrh3H2 { get; set; }
        [Column("qrh3H3")]
        public int? Qrh3H3 { get; set; }
        [Column("qrh3H4")]
        public int? Qrh3H4 { get; set; }

        // Grupo QRH 4
        [Column("qrh4H1")]
        public int? Qrh4H1 { get; set; }
        [Column("qrh4H2")]
        public int? Qrh4H2 { get; set; }
        [Column("qrh4H3")]
        public int? Qrh4H3 { get; set; }
        [Column("qrh4H4")]
        public int? Qrh4H4 { get; set; }

        // Grupo QRH 5
        [Column("qrh5H1")]
        public int? Qrh5H1 { get; set; }
        [Column("qrh5H2")]
        public int? Qrh5H2 { get; set; }
        [Column("qrh5H3")]
        public int? Qrh5H3 { get; set; }
        [Column("qrh5H4")]
        public int? Qrh5H4 { get; set; }

        // Grupo QRH 6
        [Column("qrh6H1")]
        public int? Qrh6H1 { get; set; }
        [Column("qrh6H2")]
        public int? Qrh6H2 { get; set; }
        [Column("qrh6H3")]
        public int? Qrh6H3 { get; set; }
        [Column("qrh6H4")]
        public int? Qrh6H4 { get; set; }

        // Grupo QRH 7
        [Column("qrh7H1")]
        public int? Qrh7H1 { get; set; }
        [Column("qrh7H2")]
        public int? Qrh7H2 { get; set; }
        [Column("qrh7H3")]
        public int? Qrh7H3 { get; set; }
        [Column("qrh7H4")]
        public int? Qrh7H4 { get; set; }

        // Grupo QRH 8
        [Column("qrh8H1")]
        public int? Qrh8H1 { get; set; }
        [Column("qrh8H2")]
        public int? Qrh8H2 { get; set; }
        [Column("qrh8H3")]
        public int? Qrh8H3 { get; set; }
        [Column("qrh8H4")]
        public int? Qrh8H4 { get; set; }

        // Grupo QRH 9
        [Column("qrh9H1")]
        public int? Qrh9H1 { get; set; }
        [Column("qrh9H2")]
        public int? Qrh9H2 { get; set; }
        [Column("qrh9H3")]
        public int? Qrh9H3 { get; set; }
        [Column("qrh9H4")]
        public int? Qrh9H4 { get; set; }

        // Grupo QRH 10
        [Column("qrh10H1")]
        public int? Qrh10H1 { get; set; }
        [Column("qrh10H2")]
        public int? Qrh10H2 { get; set; }
        [Column("qrh10H3")]
        public int? Qrh10H3 { get; set; }
        [Column("qrh10H4")]
        public int? Qrh10H4 { get; set; }

        // Grupo QRH 11
        [Column("qrh11H1")]
        public int? Qrh11H1 { get; set; }
        [Column("qrh11H2")]
        public int? Qrh11H2 { get; set; }
        [Column("qrh11H3")]
        public int? Qrh11H3 { get; set; }
        [Column("qrh11H4")]
        public int? Qrh11H4 { get; set; }

        // Grupo QRH 12
        [Column("qrh12H1")]
        public int? Qrh12H1 { get; set; }
        [Column("qrh12H2")]
        public int? Qrh12H2 { get; set; }
        [Column("qrh12H3")]
        public int? Qrh12H3 { get; set; }
        [Column("qrh12H4")]
        public int? Qrh12H4 { get; set; }
    }
}
