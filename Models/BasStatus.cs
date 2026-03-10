namespace WebSafeDockingAPI.Models;

/// <summary>
/// Status decodificado do BAS (Berthing Aid System).
/// Cada propriedade representa um bit do registrador de status.
/// </summary>
public class BasStatus
{
    // ---- Status Geral ----
    public bool AlarmGeral { get; set; }             // Bit 0
    public bool ModoBerthingAtivo { get; set; }      // Bit 1 (atracação em andamento)
    public bool ModoDriftingAtivo { get; set; }      // Bit 2 (navio atracado, monitorando deriva)

    // ---- Lado 1 (Side 1) ----
    public bool Side1VelocidadeOk { get; set; }      // Bit 3
    public bool Side1VelocidadeWarning { get; set; }  // Bit 4
    public bool Side1VelocidadeAlarm { get; set; }    // Bit 5
    public bool Side1DriftingWarning { get; set; }    // Bit 6
    public bool Side1DriftingAlarm { get; set; }      // Bit 7

    // ---- Lado 2 (Side 2) ----
    public bool Side2VelocidadeOk { get; set; }      // Bit 8
    public bool Side2VelocidadeWarning { get; set; }  // Bit 9
    public bool Side2VelocidadeAlarm { get; set; }    // Bit 10
    public bool Side2DriftingWarning { get; set; }    // Bit 11
    public bool Side2DriftingAlarm { get; set; }      // Bit 12
}
