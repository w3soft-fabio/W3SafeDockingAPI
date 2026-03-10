using System.ComponentModel.DataAnnotations;

namespace WebSafeDockingAPI.Models
{
    /// <summary>
    /// DTO para CREATE e UPDATE de DriftingAlarm.
    /// </summary>
    public class DriftingAlarmCreateUpdateDTO
    {
        [Required(ErrorMessage = "O campo Id é obrigatório.")]
        public int Id { get; set; }

        public decimal? MaxShipAngle { get; set; }

        public decimal? OutboundWarning { get; set; }

        public decimal? OutboundAlarm { get; set; }

        public decimal? InboundWarning { get; set; }

        public decimal? InboundAlarm { get; set; }

        /// <summary>
        /// Converte o DTO para a entidade DriftingAlarm.
        /// </summary>
        public DriftingAlarm ToEntity()
        {
            return new DriftingAlarm
            {
                Id = this.Id,
                MaxShipAngle = this.MaxShipAngle,
                OutboundWarning = this.OutboundWarning,
                OutboundAlarm = this.OutboundAlarm,
                InboundWarning = this.InboundWarning,
                InboundAlarm = this.InboundAlarm
            };
        }
    }

    /// <summary>
    /// DTO para RESPONSE de DriftingAlarm.
    /// </summary>
    public class DriftingAlarmResponseDTO
    {
        public int Id { get; set; }
        public decimal? MaxShipAngle { get; set; }
        public decimal? OutboundWarning { get; set; }
        public decimal? OutboundAlarm { get; set; }
        public decimal? InboundWarning { get; set; }
        public decimal? InboundAlarm { get; set; }

        /// <summary>
        /// Converte a entidade DriftingAlarm para DriftingAlarmResponseDTO.
        /// </summary>
        public static DriftingAlarmResponseDTO FromDriftingAlarm(DriftingAlarm alarm)
        {
            return new DriftingAlarmResponseDTO
            {
                Id = alarm.Id,
                MaxShipAngle = alarm.MaxShipAngle,
                OutboundWarning = alarm.OutboundWarning,
                OutboundAlarm = alarm.OutboundAlarm,
                InboundWarning = alarm.InboundWarning,
                InboundAlarm = alarm.InboundAlarm
            };
        }
    }
}
