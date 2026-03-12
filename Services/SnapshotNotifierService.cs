using System.Threading.Channels;
using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Services;

/// <summary>
/// Serviço de notificação publish/subscribe para snapshots Modbus.
///
/// COMO FUNCIONA:
/// - O ModbusPollingService chama NotifyAsync() a cada nova leitura.
/// - Cada cliente SSE conectado recebe uma cópia do snapshot via SubscribeAsync().
/// - Usa Channel&lt;T&gt; internamente para comunicação thread-safe entre produtores e consumidores.
///
/// CICLO DE VIDA:
/// - Registrado como Singleton no Program.cs.
/// - Quando um cliente se desconecta, seu Channel é removido automaticamente.
/// </summary>
public class SnapshotNotifierService
{
    // Lista de channels (um por cliente SSE conectado)
    private readonly List<Channel<ModbusSnapshot>> _subscribers = new();
    private readonly object _lock = new();

    /// <summary>
    /// Publica um novo snapshot para todos os clientes conectados.
    /// Chamado pelo ModbusPollingService a cada leitura.
    /// </summary>
    public void Notify(ModbusSnapshot snapshot)
    {
        lock (_lock)
        {
            // Remove subscribers cujo channel foi fechado
            _subscribers.RemoveAll(ch => ch.Reader.Completion.IsCompleted);

            foreach (var channel in _subscribers)
            {
                // TryWrite não bloqueia — se o buffer estiver cheio, descarta o snapshot antigo
                channel.Writer.TryWrite(snapshot);
            }
        }
    }

    /// <summary>
    /// Cria uma assinatura e retorna um IAsyncEnumerable que emite snapshots em tempo real.
    /// Cada chamada cria um channel independente para o cliente.
    /// </summary>
    public async IAsyncEnumerable<ModbusSnapshot> SubscribeAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Cada subscriber tem seu próprio channel com buffer de 1
        // (mantém apenas o snapshot mais recente se o cliente for lento)
        var channel = Channel.CreateBounded<ModbusSnapshot>(new BoundedChannelOptions(1)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        });

        lock (_lock)
        {
            _subscribers.Add(channel);
        }

        try
        {
            await foreach (var snapshot in channel.Reader.ReadAllAsync(cancellationToken))
            {
                yield return snapshot;
            }
        }
        finally
        {
            // Limpa o channel quando o cliente desconecta
            lock (_lock)
            {
                _subscribers.Remove(channel);
            }

            channel.Writer.TryComplete();
        }
    }
}
