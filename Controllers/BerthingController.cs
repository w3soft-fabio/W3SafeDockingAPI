using Microsoft.AspNetCore.Mvc;
using WebSafeDockingAPI.Models;
using WebSafeDockingAPI.Services;

namespace WebSafeDockingAPI.Controllers;

/// <summary>
/// Controller que expõe os dados de atracação via API REST.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BerthingController : ControllerBase
{
    private readonly IModbusReaderService _reader;
    private readonly ILogger<BerthingController> _logger;

    // Armazena o último snapshot lido (atualizado pelo Background Service)
    private static ModbusSnapshot? _lastSnapshot;

    // Lock para acesso thread-safe ao snapshot
    private static readonly object _snapshotLock = new();

    public BerthingController(
        IModbusReaderService reader,
        ILogger<BerthingController> logger)
    {
        _reader = reader;
        _logger = logger;
    }

    /// <summary>
    /// Atualiza o snapshot em memória (chamado pelo Background Service).
    /// </summary>
    internal static void UpdateSnapshot(ModbusSnapshot snapshot)
    {
        lock (_snapshotLock)
        {
            _lastSnapshot = snapshot;
        }
    }

    /// <summary>
    /// GET /api/berthing/snapshot
    /// Retorna todos os dados mais recentes do sensor.
    /// </summary>
    [HttpGet("snapshot")]
    public ActionResult<ModbusSnapshot> GetSnapshot()
    {
        lock (_snapshotLock)
        {
            if (_lastSnapshot == null)
                return NotFound("Nenhuma leitura disponível ainda. Aguarde alguns segundos.");

            return Ok(_lastSnapshot);
        }
    }

    /// <summary>
    /// GET /api/berthing/berco/1
    /// Retorna os dados de um berço específico (1 ou 2).
    /// </summary>
    [HttpGet("berco/{id:int}")]
    public ActionResult<BerthData> GetBerco(int id)
    {
        lock (_snapshotLock)
        {
            if (_lastSnapshot == null)
                return NotFound("Nenhuma leitura disponível ainda.");

            return id switch
            {
                1 => Ok(_lastSnapshot.Berco1),
                2 => Ok(_lastSnapshot.Berco2),
                _ => BadRequest("ID de berço inválido. Use 1 ou 2.")
            };
        }
    }

    /// <summary>
    /// GET /api/berthing/status
    /// Retorna apenas o status de comunicação e o Life Counter.
    /// Útil para verificações rápidas de saúde do sistema.
    /// </summary>
    [HttpGet("status")]
    public ActionResult GetStatus()
    {
        lock (_snapshotLock)
        {
            if (_lastSnapshot == null)
                return Ok(new { Conectado = false, Mensagem = "Aguardando primeira leitura..." });

            return Ok(new
            {
                Conectado = _lastSnapshot.ComunicacaoAtiva,
                UltimaLeitura = _lastSnapshot.DataHoraLeitura,
                LifeCounter = _lastSnapshot.LifeCounter,
            });
        }
    }

    /// <summary>
    /// GET /api/berthing/leitura-direta
    /// Faz uma leitura DIRETA do sensor neste exato momento (sem cache).
    /// Útil para debug, mas não use em produção para polling.
    /// </summary>
    [HttpGet("leitura-direta")]
    public async Task<ActionResult<ModbusSnapshot>> GetLeituraDireta()
    {
        try
        {
            var snapshot = await _reader.ReadAllAsync();
            return Ok(snapshot);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao realizar leitura direta do sensor.");
            return StatusCode(500, new
            {
                Erro = "Falha ao comunicar com o sensor Modbus.",
                Detalhes = ex.Message
            });
        }
    }
}
