namespace WebSafeDockingAPI.Models;

/// <summary>
/// Contém todos os dados de um berço (ponto de atracação).
/// O sistema tem 2 berços: B1 e B2.
/// </summary>
public class BerthData
{
    /// <summary>
    /// Identificação do berço ("B1" ou "B2").
    /// </summary>
    public string BercoId { get; set; } = string.Empty;

    // ---- Status ----
    public BasStatus StatusBas { get; set; } = new();
    public LddStatus StatusLdd { get; set; } = new();

    // ---- Limiares de Velocidade (Berthing Speed Thresholds) ----
    /// <summary>
    /// Velocidade limite de AVISO quando o navio está LONGE (cm/s).
    /// </summary>
    public double FarRangeWarning { get; set; }

    /// <summary>
    /// Velocidade limite de ALARME quando o navio está LONGE (cm/s).
    /// </summary>
    public double FarRangeAlarm { get; set; }

    /// <summary>
    /// Velocidade limite de AVISO em alcance MÉDIO (cm/s).
    /// </summary>
    public double MiddleRangeWarning { get; set; }

    /// <summary>
    /// Velocidade limite de ALARME em alcance MÉDIO (cm/s).
    /// </summary>
    public double MiddleRangeAlarm { get; set; }

    /// <summary>
    /// Velocidade limite de AVISO quando o navio está PERTO (cm/s).
    /// </summary>
    public double CloseRangeWarning { get; set; }

    /// <summary>
    /// Velocidade limite de ALARME quando o navio está PERTO (cm/s).
    /// </summary>
    public double CloseRangeAlarm { get; set; }

    // ---- Ângulo e Deriva ----
    /// <summary>
    /// Ângulo máximo permitido de aproximação (graus).
    /// </summary>
    public double AnguloMaximo { get; set; }

    /// <summary>
    /// Distância de deriva limite para AVISO (metros).
    /// </summary>
    public double DriftingWarning { get; set; }

    /// <summary>
    /// Distância de deriva limite para ALARME (metros).
    /// </summary>
    public double DriftingAlarm { get; set; }

    // ---- Medições em Tempo Real ----
    /// <summary>
    /// Leitura do Sensor 1 (um lado do berço).
    /// </summary>
    public SensorReading Sensor1 { get; set; } = new();

    /// <summary>
    /// Leitura do Sensor 2 (outro lado do berço).
    /// </summary>
    public SensorReading Sensor2 { get; set; } = new();

    /// <summary>
    /// Distância consolidada do navio (calculada a partir de S1 e S2) em metros.
    /// Este é o valor principal para exibição.
    /// </summary>
    public double DistanciaConsolidadaMetros { get; set; }

    /// <summary>
    /// Ângulo do navio em relação ao cais (graus).
    /// 0 = navio paralelo ao cais.
    /// </summary>
    public double AnguloNavio { get; set; }

    // ---- LDD (Laser) ----
    /// <summary>
    /// Intensidade do sinal laser (0 a 100%).
    /// Valores baixos = sinal fraco = possível imprecisão.
    /// </summary>
    public double IntensidadeLdd { get; set; }
}
