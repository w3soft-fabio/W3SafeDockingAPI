using System.ComponentModel.DataAnnotations;

namespace WebSafeDockingAPI.Models
{
    /// <summary>
    /// DTO para CREATE e UPDATE de BerthingAlarm.
    /// </summary>
    public class BerthingAlarmCreateUpdateDTO
    {
        [Required(ErrorMessage = "O campo Id é obrigatório.")]
        public int Id { get; set; }

        public decimal? MaxShipAngle { get; set; }

        public decimal? ZcMinRange { get; set; }

        public decimal? ZcMaxRange { get; set; }

        public int? ZcSpeedWarning { get; set; }

        public int? ZcSpeedAlarm { get; set; }

        public decimal? ZmMinRange { get; set; }

        public decimal? ZmMaxRange { get; set; }

        public int? ZmSpeedWarning { get; set; }

        public int? ZmSpeedAlarm { get; set; }

        public decimal? ZfMinRange { get; set; }

        public decimal? ZfMaxRange { get; set; }

        public int? ZfSpeedWarning { get; set; }

        public int? ZfSpeedAlarm { get; set; }

        /// <summary>
        /// Converte o DTO para a entidade BerthingAlarm.
        /// </summary>
        public BerthingAlarm ToEntity()
        {
            return new BerthingAlarm
            {
                Id = this.Id,
                MaxShipAngle = this.MaxShipAngle,
                ZcMinRange = this.ZcMinRange,
                ZcMaxRange = this.ZcMaxRange,
                ZcSpeedWarning = this.ZcSpeedWarning,
                ZcSpeedAlarm = this.ZcSpeedAlarm,
                ZmMinRange = this.ZmMinRange,
                ZmMaxRange = this.ZmMaxRange,
                ZmSpeedWarning = this.ZmSpeedWarning,
                ZmSpeedAlarm = this.ZmSpeedAlarm,
                ZfMinRange = this.ZfMinRange,
                ZfMaxRange = this.ZfMaxRange,
                ZfSpeedWarning = this.ZfSpeedWarning,
                ZfSpeedAlarm = this.ZfSpeedAlarm
            };
        }
    }

    /// <summary>
    /// DTO para RESPONSE de BerthingAlarm.
    /// </summary>
    public class BerthingAlarmResponseDTO
    {
        public int Id { get; set; }
        public decimal? MaxShipAngle { get; set; }
        public decimal? ZcMinRange { get; set; }
        public decimal? ZcMaxRange { get; set; }
        public int? ZcSpeedWarning { get; set; }
        public int? ZcSpeedAlarm { get; set; }
        public decimal? ZmMinRange { get; set; }
        public decimal? ZmMaxRange { get; set; }
        public int? ZmSpeedWarning { get; set; }
        public int? ZmSpeedAlarm { get; set; }
        public decimal? ZfMinRange { get; set; }
        public decimal? ZfMaxRange { get; set; }
        public int? ZfSpeedWarning { get; set; }
        public int? ZfSpeedAlarm { get; set; }

        /// <summary>
        /// Converte a entidade BerthingAlarm para BerthingAlarmResponseDTO.
        /// </summary>
        public static BerthingAlarmResponseDTO FromBerthingAlarm(BerthingAlarm alarm)
        {
            return new BerthingAlarmResponseDTO
            {
                Id = alarm.Id,
                MaxShipAngle = alarm.MaxShipAngle,
                ZcMinRange = alarm.ZcMinRange,
                ZcMaxRange = alarm.ZcMaxRange,
                ZcSpeedWarning = alarm.ZcSpeedWarning,
                ZcSpeedAlarm = alarm.ZcSpeedAlarm,
                ZmMinRange = alarm.ZmMinRange,
                ZmMaxRange = alarm.ZmMaxRange,
                ZmSpeedWarning = alarm.ZmSpeedWarning,
                ZmSpeedAlarm = alarm.ZmSpeedAlarm,
                ZfMinRange = alarm.ZfMinRange,
                ZfMaxRange = alarm.ZfMaxRange,
                ZfSpeedWarning = alarm.ZfSpeedWarning,
                ZfSpeedAlarm = alarm.ZfSpeedAlarm
            };
        }
    }
}
