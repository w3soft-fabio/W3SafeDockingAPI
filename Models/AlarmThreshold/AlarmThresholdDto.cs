using System.ComponentModel.DataAnnotations;

namespace WebSafeDockingAPI.Models
{
    /// <summary>
    /// DTO para CREATE e UPDATE de AlarmThreshold.
    /// </summary>
    public class AlarmThresholdCreateUpdateDTO
    {
        [Required(ErrorMessage = "O campo Id é obrigatório.")]
        public int Id { get; set; }

        [Required(ErrorMessage = "O campo Name é obrigatório.")]
        [StringLength(255, ErrorMessage = "O nome deve ter no máximo 255 caracteres.")]
        public string Name { get; set; } = string.Empty;

        public int? BerthingID { get; set; }

        public int? DriftingID { get; set; }

        public int? MinDwt { get; set; }

        public int? MaxDwt { get; set; }

        /// <summary>
        /// Converte o DTO para a entidade AlarmThreshold.
        /// </summary>
        public AlarmThreshold ToEntity()
        {
            return new AlarmThreshold
            {
                Id = this.Id,
                Name = this.Name,
                BerthingID = this.BerthingID,
                DriftingID = this.DriftingID,
                MinDwt = this.MinDwt,
                MaxDwt = this.MaxDwt
            };
        }
    }

    /// <summary>
    /// DTO para RESPONSE de AlarmThreshold.
    /// </summary>
    public class AlarmThresholdResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? BerthingID { get; set; }
        public BerthingAlarmResponseDTO? BerthingAlarm { get; set; }
        public int? DriftingID { get; set; }
        public DriftingAlarmResponseDTO? DriftingAlarm { get; set; }
        public int? MinDwt { get; set; }
        public int? MaxDwt { get; set; }

        /// <summary>
        /// Converte a entidade AlarmThreshold para AlarmThresholdResponseDTO.
        /// </summary>
        public static AlarmThresholdResponseDTO FromAlarmThreshold(AlarmThreshold alarmThreshold)
        {
            return new AlarmThresholdResponseDTO
            {
                Id = alarmThreshold.Id,
                Name = alarmThreshold.Name,
                BerthingID = alarmThreshold.BerthingID,
                BerthingAlarm = alarmThreshold.BerthingAlarm != null
                    ? BerthingAlarmResponseDTO.FromBerthingAlarm(alarmThreshold.BerthingAlarm)
                    : null,
                DriftingID = alarmThreshold.DriftingID,
                DriftingAlarm = alarmThreshold.DriftingAlarm != null
                    ? DriftingAlarmResponseDTO.FromDriftingAlarm(alarmThreshold.DriftingAlarm)
                    : null,
                MinDwt = alarmThreshold.MinDwt,
                MaxDwt = alarmThreshold.MaxDwt
            };
        }
    }
}
