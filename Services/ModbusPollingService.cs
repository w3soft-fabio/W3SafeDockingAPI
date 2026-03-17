using Microsoft.Extensions.Options;
using WebSafeDockingAPI.Controllers;
using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Services;

/// <summary>
/// Serviço em segundo plano que lê os dados do sensor a cada intervalo configurado.
///
/// COMO FUNCIONA:
/// - Roda automaticamente quando a aplicação inicia
/// - A cada 1 segundo (configurável), lê os registradores Modbus
/// - Armazena o resultado mais recente em memória
/// - Os endpoints da API retornam esse dado armazenado
///
/// POR QUE NÃO LER DIRETO NO ENDPOINT?
/// - Se 100 usuários fizessem requisição ao mesmo tempo, o sensor
///   receberia 100 leituras simultâneas — isso pode sobrecarregá-lo.
/// - Com o Background Service, fazemos apenas 1 leitura por segundo,
///   independente de quantos usuários estão acessando a API.
/// </summary>
public class ModbusPollingService : BackgroundService
{
    private readonly IModbusReaderService _reader;
    private readonly ILogger<ModbusPollingService> _logger;
    private readonly ModbusSettings _settings;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SnapshotNotifierService _notifier;

    // Para detectar perda de comunicação
    private int _lastLifeCounter = -1;
    private int _sameLifeCounterCount = 0;

    public ModbusPollingService(
        IModbusReaderService reader,
        IOptions<ModbusSettings> settings,
        ILogger<ModbusPollingService> logger,
        IServiceScopeFactory scopeFactory,
        SnapshotNotifierService notifier)
    {
        _reader = reader;
        _settings = settings.Value;
        _logger = logger;
        _scopeFactory = scopeFactory;
        _notifier = notifier;
    }

    /// <summary>
    /// Método executado automaticamente pelo .NET quando a aplicação inicia.
    /// Roda em loop até a aplicação ser encerrada.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Serviço de polling Modbus iniciado (intervalo: {Interval}ms)",
            _settings.ReadIntervalMs);

        // Loop infinito que roda até a aplicação ser encerrada
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Lê todos os dados do sensor
                var snapshot = await _reader.ReadAllAsync();

                // Verifica se o Life Counter está mudando
                VerificarComunicacao(snapshot);

                // Atualiza o dado em memória (que a API vai retornar)
                BerthingController.UpdateSnapshot(snapshot);

                // Notifica todos os clientes SSE conectados
                _notifier.Notify(snapshot);

                // Persiste o snapshot no banco de dados
                await SalvarSnapshotAsync(snapshot);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Falha na leitura Modbus. Tentando novamente em {Interval}ms...",
                    _settings.ReadIntervalMs);

                // Em caso de erro, marca como sem comunicação
                BerthingController.UpdateSnapshot(new ModbusSnapshot
                {
                    DataHoraLeitura = DateTime.UtcNow,
                    ComunicacaoAtiva = false,
                });
            }

            // Aguarda o intervalo antes da próxima leitura
            await Task.Delay(_settings.ReadIntervalMs, stoppingToken);
        }

        _logger.LogInformation("Serviço de polling Modbus encerrado.");
    }

    /// <summary>
    /// Verifica se a comunicação está ativa checando o Life Counter.
    /// Se o valor não muda por 3 ciclos seguidos, a comunicação foi perdida.
    /// </summary>
    private void VerificarComunicacao(ModbusSnapshot snapshot)
    {
        if (snapshot.LifeCounter == _lastLifeCounter)
        {
            _sameLifeCounterCount++;

            if (_sameLifeCounterCount >= 3)
            {
                snapshot.ComunicacaoAtiva = false;
                _logger.LogWarning(
                    "Life Counter parado em {Value} por {Count} ciclos — possível perda de comunicação!",
                    _lastLifeCounter,
                    _sameLifeCounterCount);
            }
        }
        else
        {
            _sameLifeCounterCount = 0;
            snapshot.ComunicacaoAtiva = true;
        }

        _lastLifeCounter = snapshot.LifeCounter;
    }

    /// <summary>
    /// Persiste o snapshot no banco usando um escopo de DI (scoped service dentro de singleton).
    /// </summary>
    private async Task SalvarSnapshotAsync(ModbusSnapshot snapshot)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<BerthSnapshotService>();
            await service.SaveSnapshotAsync(snapshot);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao salvar snapshot no banco de dados.");
        }
    }
}
