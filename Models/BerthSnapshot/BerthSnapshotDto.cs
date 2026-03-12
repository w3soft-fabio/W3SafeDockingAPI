using WebSafeDockingAPI.Models.Common;

namespace WebSafeDockingAPI.Models
{
    /// <summary>
    /// Parâmetros de busca para snapshots (estende paginação padrão).
    /// </summary>
    public class BerthSnapshotSearchRequest : PaginationRequest
    {
        /// <summary>
        /// Filtro por berço ("B1" ou "B2"). Opcional.
        /// </summary>
        public string? BercoId { get; set; }

        /// <summary>
        /// Data/hora inicial do intervalo. Opcional.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Data/hora final do intervalo. Opcional.
        /// </summary>
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// DTO de resposta para BerthSnapshot.
    /// </summary>
    public class BerthSnapshotResponseDTO
    {
        public long Id { get; set; }
        public DateTime CapturedAt { get; set; }
        public string BercoId { get; set; } = string.Empty;

        // Limiares
        public double FarRangeWarning { get; set; }
        public double FarRangeAlarm { get; set; }
        public double MiddleRangeWarning { get; set; }
        public double MiddleRangeAlarm { get; set; }
        public double CloseRangeWarning { get; set; }
        public double CloseRangeAlarm { get; set; }

        // Ângulo e Deriva
        public double AnguloMaximo { get; set; }
        public double DriftingWarning { get; set; }
        public double DriftingAlarm { get; set; }

        // Medições
        public double DistanciaConsolidadaMetros { get; set; }
        public double AnguloNavio { get; set; }
        public double IntensidadeLdd { get; set; }

        // Sensores
        public double S1DistanciaMetros { get; set; }
        public double S1VelocidadeCmPorSegundo { get; set; }
        public double S2DistanciaMetros { get; set; }
        public double S2VelocidadeCmPorSegundo { get; set; }

        // BAS Status
        public bool BasAlarmGeral { get; set; }
        public bool BasModoBerthingAtivo { get; set; }
        public bool BasModoDriftingAtivo { get; set; }
        public bool BasSide1VelocidadeOk { get; set; }
        public bool BasSide1VelocidadeWarning { get; set; }
        public bool BasSide1VelocidadeAlarm { get; set; }
        public bool BasSide1DriftingWarning { get; set; }
        public bool BasSide1DriftingAlarm { get; set; }
        public bool BasSide2VelocidadeOk { get; set; }
        public bool BasSide2VelocidadeWarning { get; set; }
        public bool BasSide2VelocidadeAlarm { get; set; }
        public bool BasSide2DriftingWarning { get; set; }
        public bool BasSide2DriftingAlarm { get; set; }

        // LDD Status
        public bool LddAlarmGeral { get; set; }
        public bool LddAlarmComunicacao { get; set; }

        public static BerthSnapshotResponseDTO FromEntity(BerthSnapshot entity)
        {
            return new BerthSnapshotResponseDTO
            {
                Id = entity.Id,
                CapturedAt = entity.CapturedAt,
                BercoId = entity.BercoId,

                FarRangeWarning = entity.FarRangeWarning,
                FarRangeAlarm = entity.FarRangeAlarm,
                MiddleRangeWarning = entity.MiddleRangeWarning,
                MiddleRangeAlarm = entity.MiddleRangeAlarm,
                CloseRangeWarning = entity.CloseRangeWarning,
                CloseRangeAlarm = entity.CloseRangeAlarm,

                AnguloMaximo = entity.AnguloMaximo,
                DriftingWarning = entity.DriftingWarning,
                DriftingAlarm = entity.DriftingAlarm,

                DistanciaConsolidadaMetros = entity.DistanciaConsolidadaMetros,
                AnguloNavio = entity.AnguloNavio,
                IntensidadeLdd = entity.IntensidadeLdd,

                S1DistanciaMetros = entity.S1DistanciaMetros,
                S1VelocidadeCmPorSegundo = entity.S1VelocidadeCmPorSegundo,
                S2DistanciaMetros = entity.S2DistanciaMetros,
                S2VelocidadeCmPorSegundo = entity.S2VelocidadeCmPorSegundo,

                BasAlarmGeral = entity.BasAlarmGeral,
                BasModoBerthingAtivo = entity.BasModoBerthingAtivo,
                BasModoDriftingAtivo = entity.BasModoDriftingAtivo,
                BasSide1VelocidadeOk = entity.BasSide1VelocidadeOk,
                BasSide1VelocidadeWarning = entity.BasSide1VelocidadeWarning,
                BasSide1VelocidadeAlarm = entity.BasSide1VelocidadeAlarm,
                BasSide1DriftingWarning = entity.BasSide1DriftingWarning,
                BasSide1DriftingAlarm = entity.BasSide1DriftingAlarm,
                BasSide2VelocidadeOk = entity.BasSide2VelocidadeOk,
                BasSide2VelocidadeWarning = entity.BasSide2VelocidadeWarning,
                BasSide2VelocidadeAlarm = entity.BasSide2VelocidadeAlarm,
                BasSide2DriftingWarning = entity.BasSide2DriftingWarning,
                BasSide2DriftingAlarm = entity.BasSide2DriftingAlarm,

                LddAlarmGeral = entity.LddAlarmGeral,
                LddAlarmComunicacao = entity.LddAlarmComunicacao,
            };
        }
    }
}
