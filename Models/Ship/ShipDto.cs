using System.ComponentModel.DataAnnotations;

namespace WebSafeDockingAPI.Models
{
    /// <summary>
    /// DTO para CREATE e UPDATE de Ship.
    /// </summary>
    public class ShipCreateUpdateDTO
    {

        [Required(ErrorMessage = "O campo IMO é obrigatório.")]
        public int Imo { get; set; }

        [StringLength(255, ErrorMessage = "O nome deve ter no máximo 255 caracteres.")]
        public string? Name { get; set; }

        public int? Length { get; set; }

        public int? Width { get; set; }

        public int? Dwt { get; set; }

        public int? AlarmID { get; set; }

        /// <summary>
        /// Converte o DTO para a entidade Ship.
        /// </summary>
        public Ship ToEntity()
        {
            return new Ship
            {
                Imo = this.Imo,
                Name = this.Name,
                Length = this.Length,
                Width = this.Width,
                Dwt = this.Dwt,
                AlarmID = this.AlarmID
            };
        }
    }

    /// <summary>
    /// DTO para RESPONSE de Ship.
    /// </summary>
    public class ShipResponseDTO
    {
        public int Id { get; set; }
        public int Imo { get; set; }
        public string? Name { get; set; }
        public int? Length { get; set; }
        public int? Width { get; set; }
        public int? Dwt { get; set; }
        public int? AlarmID { get; set; }

        /// <summary>
        /// Converte a entidade Ship para ShipResponseDTO.
        /// </summary>
        public static ShipResponseDTO FromShip(Ship ship)
        {
            return new ShipResponseDTO
            {
                Id = ship.Id,
                Imo = ship.Imo,
                Name = ship.Name,
                Length = ship.Length,
                Width = ship.Width,
                Dwt = ship.Dwt,
                AlarmID = ship.AlarmID
            };
        }
    }
}
