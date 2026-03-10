namespace WebSafeDockingAPI.Models;

/// <summary>
/// Status decodificado do LDD (Laser Distance Detection).
/// </summary>
public class LddStatus
{
    public bool AlarmGeral { get; set; }           // Bit 0
    public bool AlarmComunicacao { get; set; }     // Bit 1
}
