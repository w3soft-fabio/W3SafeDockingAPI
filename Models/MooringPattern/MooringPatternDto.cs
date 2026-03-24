using System.ComponentModel.DataAnnotations;

namespace WebSafeDockingAPI.Models
{
    /// <summary>
    /// DTO para CREATE e UPDATE de MooringPattern.
    /// </summary>
    public class MooringPatternCreateUpdateDTO
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(255, ErrorMessage = "O nome deve ter no máximo 255 caracteres.")]
        public string Name { get; set; } = string.Empty;

        public int? JettyID { get; set; }

        // Grupo QRH 1
        public int? Qrh1H1 { get; set; }
        public int? Qrh1H2 { get; set; }
        public int? Qrh1H3 { get; set; }
        public int? Qrh1H4 { get; set; }

        // Grupo QRH 2
        public int? Qrh2H1 { get; set; }
        public int? Qrh2H2 { get; set; }
        public int? Qrh2H3 { get; set; }
        public int? Qrh2H4 { get; set; }

        // Grupo QRH 3
        public int? Qrh3H1 { get; set; }
        public int? Qrh3H2 { get; set; }
        public int? Qrh3H3 { get; set; }
        public int? Qrh3H4 { get; set; }

        // Grupo QRH 4
        public int? Qrh4H1 { get; set; }
        public int? Qrh4H2 { get; set; }
        public int? Qrh4H3 { get; set; }
        public int? Qrh4H4 { get; set; }

        // Grupo QRH 5
        public int? Qrh5H1 { get; set; }
        public int? Qrh5H2 { get; set; }
        public int? Qrh5H3 { get; set; }
        public int? Qrh5H4 { get; set; }

        // Grupo QRH 6
        public int? Qrh6H1 { get; set; }
        public int? Qrh6H2 { get; set; }
        public int? Qrh6H3 { get; set; }
        public int? Qrh6H4 { get; set; }

        // Grupo QRH 7
        public int? Qrh7H1 { get; set; }
        public int? Qrh7H2 { get; set; }
        public int? Qrh7H3 { get; set; }
        public int? Qrh7H4 { get; set; }

        // Grupo QRH 8
        public int? Qrh8H1 { get; set; }
        public int? Qrh8H2 { get; set; }
        public int? Qrh8H3 { get; set; }
        public int? Qrh8H4 { get; set; }

        // Grupo QRH 9
        public int? Qrh9H1 { get; set; }
        public int? Qrh9H2 { get; set; }
        public int? Qrh9H3 { get; set; }
        public int? Qrh9H4 { get; set; }

        // Grupo QRH 10
        public int? Qrh10H1 { get; set; }
        public int? Qrh10H2 { get; set; }
        public int? Qrh10H3 { get; set; }
        public int? Qrh10H4 { get; set; }

        // Grupo QRH 11
        public int? Qrh11H1 { get; set; }
        public int? Qrh11H2 { get; set; }
        public int? Qrh11H3 { get; set; }
        public int? Qrh11H4 { get; set; }

        // Grupo QRH 12
        public int? Qrh12H1 { get; set; }
        public int? Qrh12H2 { get; set; }
        public int? Qrh12H3 { get; set; }
        public int? Qrh12H4 { get; set; }

        /// <summary>
        /// Converte o DTO para a entidade MooringPattern.
        /// </summary>
        public MooringPattern ToEntity()
        {
            return new MooringPattern
            {
                Name = this.Name,
                JettyID = this.JettyID,
                Qrh1H1 = this.Qrh1H1, Qrh1H2 = this.Qrh1H2, Qrh1H3 = this.Qrh1H3, Qrh1H4 = this.Qrh1H4,
                Qrh2H1 = this.Qrh2H1, Qrh2H2 = this.Qrh2H2, Qrh2H3 = this.Qrh2H3, Qrh2H4 = this.Qrh2H4,
                Qrh3H1 = this.Qrh3H1, Qrh3H2 = this.Qrh3H2, Qrh3H3 = this.Qrh3H3, Qrh3H4 = this.Qrh3H4,
                Qrh4H1 = this.Qrh4H1, Qrh4H2 = this.Qrh4H2, Qrh4H3 = this.Qrh4H3, Qrh4H4 = this.Qrh4H4,
                Qrh5H1 = this.Qrh5H1, Qrh5H2 = this.Qrh5H2, Qrh5H3 = this.Qrh5H3, Qrh5H4 = this.Qrh5H4,
                Qrh6H1 = this.Qrh6H1, Qrh6H2 = this.Qrh6H2, Qrh6H3 = this.Qrh6H3, Qrh6H4 = this.Qrh6H4,
                Qrh7H1 = this.Qrh7H1, Qrh7H2 = this.Qrh7H2, Qrh7H3 = this.Qrh7H3, Qrh7H4 = this.Qrh7H4,
                Qrh8H1 = this.Qrh8H1, Qrh8H2 = this.Qrh8H2, Qrh8H3 = this.Qrh8H3, Qrh8H4 = this.Qrh8H4,
                Qrh9H1 = this.Qrh9H1, Qrh9H2 = this.Qrh9H2, Qrh9H3 = this.Qrh9H3, Qrh9H4 = this.Qrh9H4,
                Qrh10H1 = this.Qrh10H1, Qrh10H2 = this.Qrh10H2, Qrh10H3 = this.Qrh10H3, Qrh10H4 = this.Qrh10H4,
                Qrh11H1 = this.Qrh11H1, Qrh11H2 = this.Qrh11H2, Qrh11H3 = this.Qrh11H3, Qrh11H4 = this.Qrh11H4,
                Qrh12H1 = this.Qrh12H1, Qrh12H2 = this.Qrh12H2, Qrh12H3 = this.Qrh12H3, Qrh12H4 = this.Qrh12H4
            };
        }
    }

    /// <summary>
    /// DTO para RESPONSE de MooringPattern.
    /// </summary>
    public class MooringPatternResponseDTO
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? JettyID { get; set; }

        // Grupo QRH 1
        public int? Qrh1H1 { get; set; }
        public int? Qrh1H2 { get; set; }
        public int? Qrh1H3 { get; set; }
        public int? Qrh1H4 { get; set; }

        // Grupo QRH 2
        public int? Qrh2H1 { get; set; }
        public int? Qrh2H2 { get; set; }
        public int? Qrh2H3 { get; set; }
        public int? Qrh2H4 { get; set; }

        // Grupo QRH 3
        public int? Qrh3H1 { get; set; }
        public int? Qrh3H2 { get; set; }
        public int? Qrh3H3 { get; set; }
        public int? Qrh3H4 { get; set; }

        // Grupo QRH 4
        public int? Qrh4H1 { get; set; }
        public int? Qrh4H2 { get; set; }
        public int? Qrh4H3 { get; set; }
        public int? Qrh4H4 { get; set; }

        // Grupo QRH 5
        public int? Qrh5H1 { get; set; }
        public int? Qrh5H2 { get; set; }
        public int? Qrh5H3 { get; set; }
        public int? Qrh5H4 { get; set; }

        // Grupo QRH 6
        public int? Qrh6H1 { get; set; }
        public int? Qrh6H2 { get; set; }
        public int? Qrh6H3 { get; set; }
        public int? Qrh6H4 { get; set; }

        // Grupo QRH 7
        public int? Qrh7H1 { get; set; }
        public int? Qrh7H2 { get; set; }
        public int? Qrh7H3 { get; set; }
        public int? Qrh7H4 { get; set; }

        // Grupo QRH 8
        public int? Qrh8H1 { get; set; }
        public int? Qrh8H2 { get; set; }
        public int? Qrh8H3 { get; set; }
        public int? Qrh8H4 { get; set; }

        // Grupo QRH 9
        public int? Qrh9H1 { get; set; }
        public int? Qrh9H2 { get; set; }
        public int? Qrh9H3 { get; set; }
        public int? Qrh9H4 { get; set; }

        // Grupo QRH 10
        public int? Qrh10H1 { get; set; }
        public int? Qrh10H2 { get; set; }
        public int? Qrh10H3 { get; set; }
        public int? Qrh10H4 { get; set; }

        // Grupo QRH 11
        public int? Qrh11H1 { get; set; }
        public int? Qrh11H2 { get; set; }
        public int? Qrh11H3 { get; set; }
        public int? Qrh11H4 { get; set; }

        // Grupo QRH 12
        public int? Qrh12H1 { get; set; }
        public int? Qrh12H2 { get; set; }
        public int? Qrh12H3 { get; set; }
        public int? Qrh12H4 { get; set; }

        /// <summary>
        /// Converte a entidade MooringPattern para MooringPatternResponseDTO.
        /// </summary>
        public static MooringPatternResponseDTO FromMooringPattern(MooringPattern mp)
        {
            return new MooringPatternResponseDTO
            {
                Id = mp.Id,
                Name = mp.Name,
                JettyID = mp.JettyID,
                Qrh1H1 = mp.Qrh1H1, Qrh1H2 = mp.Qrh1H2, Qrh1H3 = mp.Qrh1H3, Qrh1H4 = mp.Qrh1H4,
                Qrh2H1 = mp.Qrh2H1, Qrh2H2 = mp.Qrh2H2, Qrh2H3 = mp.Qrh2H3, Qrh2H4 = mp.Qrh2H4,
                Qrh3H1 = mp.Qrh3H1, Qrh3H2 = mp.Qrh3H2, Qrh3H3 = mp.Qrh3H3, Qrh3H4 = mp.Qrh3H4,
                Qrh4H1 = mp.Qrh4H1, Qrh4H2 = mp.Qrh4H2, Qrh4H3 = mp.Qrh4H3, Qrh4H4 = mp.Qrh4H4,
                Qrh5H1 = mp.Qrh5H1, Qrh5H2 = mp.Qrh5H2, Qrh5H3 = mp.Qrh5H3, Qrh5H4 = mp.Qrh5H4,
                Qrh6H1 = mp.Qrh6H1, Qrh6H2 = mp.Qrh6H2, Qrh6H3 = mp.Qrh6H3, Qrh6H4 = mp.Qrh6H4,
                Qrh7H1 = mp.Qrh7H1, Qrh7H2 = mp.Qrh7H2, Qrh7H3 = mp.Qrh7H3, Qrh7H4 = mp.Qrh7H4,
                Qrh8H1 = mp.Qrh8H1, Qrh8H2 = mp.Qrh8H2, Qrh8H3 = mp.Qrh8H3, Qrh8H4 = mp.Qrh8H4,
                Qrh9H1 = mp.Qrh9H1, Qrh9H2 = mp.Qrh9H2, Qrh9H3 = mp.Qrh9H3, Qrh9H4 = mp.Qrh9H4,
                Qrh10H1 = mp.Qrh10H1, Qrh10H2 = mp.Qrh10H2, Qrh10H3 = mp.Qrh10H3, Qrh10H4 = mp.Qrh10H4,
                Qrh11H1 = mp.Qrh11H1, Qrh11H2 = mp.Qrh11H2, Qrh11H3 = mp.Qrh11H3, Qrh11H4 = mp.Qrh11H4,
                Qrh12H1 = mp.Qrh12H1, Qrh12H2 = mp.Qrh12H2, Qrh12H3 = mp.Qrh12H3, Qrh12H4 = mp.Qrh12H4
            };
        }
    }
}
