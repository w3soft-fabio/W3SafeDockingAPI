using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebSafeDockingAPI.Models
{
    [Table("mooring_companies")]
    public class MooringCompany
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("mooringCompanyID")]
        public int MooringCompanyID { get; set; }

        [Required]
        [StringLength(150)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;
    }
}
