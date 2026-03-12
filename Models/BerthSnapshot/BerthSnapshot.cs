using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebSafeDockingAPI.Models
{
    [Table("berth_snapshot")]
    public class BerthSnapshot
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public long Id { get; set; }

        [Column("captured_at")]
        public DateTime CapturedAt { get; set; }

        [Required]
        [StringLength(10)]
        [Column("berco_id")]
        public string BercoId { get; set; } = string.Empty;

        // ---- Limiares de velocidade ----
        [Column("far_range_warning")]
        public double FarRangeWarning { get; set; }

        [Column("far_range_alarm")]
        public double FarRangeAlarm { get; set; }

        [Column("middle_range_warning")]
        public double MiddleRangeWarning { get; set; }

        [Column("middle_range_alarm")]
        public double MiddleRangeAlarm { get; set; }

        [Column("close_range_warning")]
        public double CloseRangeWarning { get; set; }

        [Column("close_range_alarm")]
        public double CloseRangeAlarm { get; set; }

        // ---- Ângulo e Deriva ----
        [Column("angulo_maximo")]
        public double AnguloMaximo { get; set; }

        [Column("drifting_warning")]
        public double DriftingWarning { get; set; }

        [Column("drifting_alarm")]
        public double DriftingAlarm { get; set; }

        // ---- Medições consolidadas ----
        [Column("distancia_consolidada_metros")]
        public double DistanciaConsolidadaMetros { get; set; }

        [Column("angulo_navio")]
        public double AnguloNavio { get; set; }

        [Column("intensidade_ldd")]
        public double IntensidadeLdd { get; set; }

        // ---- Sensor 1 ----
        [Column("s1_distancia_metros")]
        public double S1DistanciaMetros { get; set; }

        [Column("s1_velocidade_cm_por_segundo")]
        public double S1VelocidadeCmPorSegundo { get; set; }

        // ---- Sensor 2 ----
        [Column("s2_distancia_metros")]
        public double S2DistanciaMetros { get; set; }

        [Column("s2_velocidade_cm_por_segundo")]
        public double S2VelocidadeCmPorSegundo { get; set; }

        // ---- BAS Status ----
        [Column("bas_alarm_geral")]
        public bool BasAlarmGeral { get; set; }

        [Column("bas_modo_berthing_ativo")]
        public bool BasModoBerthingAtivo { get; set; }

        [Column("bas_modo_drifting_ativo")]
        public bool BasModoDriftingAtivo { get; set; }

        [Column("bas_side1_velocidade_ok")]
        public bool BasSide1VelocidadeOk { get; set; }

        [Column("bas_side1_velocidade_warning")]
        public bool BasSide1VelocidadeWarning { get; set; }

        [Column("bas_side1_velocidade_alarm")]
        public bool BasSide1VelocidadeAlarm { get; set; }

        [Column("bas_side1_drifting_warning")]
        public bool BasSide1DriftingWarning { get; set; }

        [Column("bas_side1_drifting_alarm")]
        public bool BasSide1DriftingAlarm { get; set; }

        [Column("bas_side2_velocidade_ok")]
        public bool BasSide2VelocidadeOk { get; set; }

        [Column("bas_side2_velocidade_warning")]
        public bool BasSide2VelocidadeWarning { get; set; }

        [Column("bas_side2_velocidade_alarm")]
        public bool BasSide2VelocidadeAlarm { get; set; }

        [Column("bas_side2_drifting_warning")]
        public bool BasSide2DriftingWarning { get; set; }

        [Column("bas_side2_drifting_alarm")]
        public bool BasSide2DriftingAlarm { get; set; }

        // ---- LDD Status ----
        [Column("ldd_alarm_geral")]
        public bool LddAlarmGeral { get; set; }

        [Column("ldd_alarm_comunicacao")]
        public bool LddAlarmComunicacao { get; set; }

        /// <summary>
        /// Cria um BerthSnapshot a partir de um BerthData e timestamp.
        /// </summary>
        public static BerthSnapshot FromBerthData(BerthData berth, DateTime capturedAt)
        {
            return new BerthSnapshot
            {
                CapturedAt = capturedAt,
                BercoId = berth.BercoId,

                FarRangeWarning = berth.FarRangeWarning,
                FarRangeAlarm = berth.FarRangeAlarm,
                MiddleRangeWarning = berth.MiddleRangeWarning,
                MiddleRangeAlarm = berth.MiddleRangeAlarm,
                CloseRangeWarning = berth.CloseRangeWarning,
                CloseRangeAlarm = berth.CloseRangeAlarm,

                AnguloMaximo = berth.AnguloMaximo,
                DriftingWarning = berth.DriftingWarning,
                DriftingAlarm = berth.DriftingAlarm,

                DistanciaConsolidadaMetros = berth.DistanciaConsolidadaMetros,
                AnguloNavio = berth.AnguloNavio,
                IntensidadeLdd = berth.IntensidadeLdd,

                S1DistanciaMetros = berth.Sensor1.DistanciaMetros,
                S1VelocidadeCmPorSegundo = berth.Sensor1.VelocidadeCmPorSegundo,
                S2DistanciaMetros = berth.Sensor2.DistanciaMetros,
                S2VelocidadeCmPorSegundo = berth.Sensor2.VelocidadeCmPorSegundo,

                BasAlarmGeral = berth.StatusBas.AlarmGeral,
                BasModoBerthingAtivo = berth.StatusBas.ModoBerthingAtivo,
                BasModoDriftingAtivo = berth.StatusBas.ModoDriftingAtivo,
                BasSide1VelocidadeOk = berth.StatusBas.Side1VelocidadeOk,
                BasSide1VelocidadeWarning = berth.StatusBas.Side1VelocidadeWarning,
                BasSide1VelocidadeAlarm = berth.StatusBas.Side1VelocidadeAlarm,
                BasSide1DriftingWarning = berth.StatusBas.Side1DriftingWarning,
                BasSide1DriftingAlarm = berth.StatusBas.Side1DriftingAlarm,
                BasSide2VelocidadeOk = berth.StatusBas.Side2VelocidadeOk,
                BasSide2VelocidadeWarning = berth.StatusBas.Side2VelocidadeWarning,
                BasSide2VelocidadeAlarm = berth.StatusBas.Side2VelocidadeAlarm,
                BasSide2DriftingWarning = berth.StatusBas.Side2DriftingWarning,
                BasSide2DriftingAlarm = berth.StatusBas.Side2DriftingAlarm,

                LddAlarmGeral = berth.StatusLdd.AlarmGeral,
                LddAlarmComunicacao = berth.StatusLdd.AlarmComunicacao,
            };
        }
    }
}
