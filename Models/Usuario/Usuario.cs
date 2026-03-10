using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebSafeDockingAPI.Models
{
    [Table("usuarios")]
    public class Usuario
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("id")]
        public int Id { get; set; }

        [StringLength(100)]
        [Column("nome")]
        public string? Nome { get; set; }

        [StringLength(14)]
        [Column("cpf")]
        public string? Cpf { get; set; }

        [StringLength(50)]
        [Column("nivelAcesso")]
        public string? NivelAcesso { get; set; }

        [StringLength(255)]
        [Column("senhaHash")]
        public string? SenhaHash { get; set; }

        [StringLength(15)]
        [Column("telefone")]
        public string? Telefone { get; set; }

        [StringLength(100)]
        [Column("email")]
        public string? Email { get; set; }

        [StringLength(250)]
        [Column("razaoSocial")]
        public string? RazaoSocial { get; set; }

        [Column("ativo")]
        public bool? Ativo { get; set; }
    }
}
