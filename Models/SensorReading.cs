namespace WebSafeDockingAPI.Models;

/// <summary>
/// Representa a leitura de um sensor individual (S1 ou S2).
/// Cada berço tem dois sensores — um de cada lado.
/// </summary>
public class SensorReading
{
    /// <summary>
    /// Distância do navio ao cais medida por este sensor (em metros).
    /// Valores negativos = navio se afastando.
    /// </summary>
    public double DistanciaMetros { get; set; }

    /// <summary>
    /// Velocidade de aproximação do navio (em cm/s).
    /// </summary>
    public double VelocidadeCmPorSegundo { get; set; }
}
