namespace WebSafeDockingAPI.Services;

/// <summary>
/// Interface que define o contrato do serviço de conexão Modbus.
/// Usar interfaces facilita os testes e a manutenção do código.
/// </summary>
public interface IModbusConnectionService
{
    /// <summary>
    /// Lê um bloco de registradores Holding do sensor.
    /// </summary>
    /// <param name="startAddress">Endereço inicial (ex: 1000).</param>
    /// <param name="numberOfRegisters">Quantos registradores ler (ex: 37).</param>
    /// <returns>Array com os valores brutos (ushort = 0 a 65535).</returns>
    Task<ushort[]> ReadHoldingRegistersAsync(ushort startAddress, ushort numberOfRegisters);

    /// <summary>
    /// Verifica se a conexão está ativa.
    /// </summary>
    bool IsConnected { get; }
}
