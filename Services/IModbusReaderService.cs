using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Services;

public interface IModbusReaderService
{
    /// <summary>
    /// Lê todos os registradores e retorna um snapshot organizado.
    /// </summary>
    Task<ModbusSnapshot> ReadAllAsync();
}
