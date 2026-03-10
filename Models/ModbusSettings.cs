namespace WebSafeDockingAPI.Models;

/// <summary>
/// Configurações de conexão Modbus — mapeadas do appsettings.json.
/// </summary>
public class ModbusSettings
{
    /// <summary>
    /// IP do sensor Modbus.
    /// </summary>
    public string IpAddress { get; set; } = "192.168.54.101";

    /// <summary>
    /// Porta TCP (padrão Modbus = 502).
    /// </summary>
    public int Port { get; set; } = 502;

    /// <summary>
    /// ID do dispositivo Modbus (Slave ID).
    /// </summary>
    public byte SlaveId { get; set; } = 1;

    /// <summary>
    /// Intervalo entre leituras em milissegundos.
    /// </summary>
    public int ReadIntervalMs { get; set; } = 1000;
}
