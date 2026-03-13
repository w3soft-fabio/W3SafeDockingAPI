# Como Funciona o Streaming (SSE) neste Projeto — Guia Completo

## Índice

1. [O que é SSE (Server-Sent Events)?](#1-o-que-é-sse-server-sent-events)
2. [Como o SSE se compara a outras abordagens?](#2-como-o-sse-se-compara-a-outras-abordagens)
3. [Visão Geral da Arquitetura](#3-visão-geral-da-arquitetura)
4. [Peça 1 — O Notificador (SnapshotNotifierService)](#4-peça-1--o-notificador-snapshotnotifierservice)
5. [Peça 2 — O Produtor de Dados (ModbusPollingService)](#5-peça-2--o-produtor-de-dados-modbuspollingservice)
6. [Peça 3 — O Endpoint SSE (BerthingController)](#6-peça-3--o-endpoint-sse-berthingcontroller)
7. [Peça 4 — Registro no Program.cs](#7-peça-4--registro-no-programcs)
8. [Fluxo Completo Passo a Passo](#8-fluxo-completo-passo-a-passo)
9. [Conceitos-chave do C# usados](#9-conceitos-chave-do-c-usados)
10. [Como criar do zero uma feature de streaming semelhante](#10-como-criar-do-zero-uma-feature-de-streaming-semelhante)
11. [Testando o Endpoint SSE](#11-testando-o-endpoint-sse)
12. [Erros Comuns e Troubleshooting](#12-erros-comuns-e-troubleshooting)

---

## 1. O que é SSE (Server-Sent Events)?

**SSE** é uma tecnologia web simples que permite ao **servidor enviar dados para o cliente automaticamente**, sem que o cliente precise ficar perguntando "tem algo novo?".

### Analogia

Imagine que você está em um restaurante:

- **Polling (HTTP normal):** Você vai até o balcão a cada 1 segundo e pergunta "Meu pedido tá pronto?". Mesmo que não esteja, você gasta energia indo e voltando.
- **SSE (streaming):** Você senta na mesa e o garçom **vem até você** assim que o pedido fica pronto. Você não precisa fazer nada — só esperar.

### Como funciona na prática?

1. O cliente faz **uma única requisição HTTP GET** para o servidor.
2. O servidor **não fecha a conexão**. Em vez disso, mantém a conexão aberta.
3. Toda vez que tem um dado novo, o servidor **escreve uma linha** na resposta.
4. O cliente recebe cada linha assim que ela é enviada.

O formato de cada mensagem SSE é simples:

```
data: {"temperatura": 25.3, "hora": "14:30:00"}\n\n
```

- Começa com `data: `
- Seguido do conteúdo (geralmente JSON)
- Termina com **duas quebras de linha** (`\n\n`) para indicar o fim do evento

---

## 2. Como o SSE se compara a outras abordagens?

| Característica       | Polling (HTTP normal)          | WebSocket                        | SSE                              |
|----------------------|--------------------------------|----------------------------------|----------------------------------|
| Direção              | Cliente → Servidor             | Bidirecional                     | Servidor → Cliente               |
| Complexidade         | Simples                        | Complexa                         | Simples                          |
| Protocolo            | HTTP                           | Protocolo próprio (ws://)        | HTTP (padrão)                    |
| Reconexão automática | Não                            | Precisa implementar              | Sim (no navegador)               |
| Uso ideal            | Dados que mudam pouco          | Chat, jogos                      | Dashboards, monitoramento        |

**Por que escolhemos SSE neste projeto?**

- Os dados fluem apenas do servidor para o cliente (sensor → app).
- SSE usa HTTP padrão — mais simples de configurar e funciona com autenticação JWT.
- Ideal para dashboards de monitoramento em tempo real.

---

## 3. Visão Geral da Arquitetura

O streaming neste projeto envolve **4 peças** que trabalham juntas:

```
┌──────────────────────┐
│  Sensor (Modbus)     │   ← fonte dos dados (lido a cada ~1 segundo)
└──────────┬───────────┘
           │
           ▼
┌──────────────────────┐
│ ModbusPollingService │   ← Background Service: lê o sensor em loop
│ (Produtor)           │
└──────────┬───────────┘
           │  chama .Notify(snapshot)
           ▼
┌──────────────────────────┐
│ SnapshotNotifierService  │   ← Pub/Sub: distribui dados para N clientes
│ (Roteador)               │
└──────┬─────────┬─────────┘
       │         │          chama .SubscribeAsync()
       ▼         ▼
┌───────────┐ ┌───────────┐
│ Cliente 1 │ │ Cliente 2 │   ← Cada conexão SSE é um subscriber
│ (SSE)     │ │ (SSE)     │
└───────────┘ └───────────┘
```

**Resumo:**

| Peça                       | Arquivo                         | Responsabilidade                                      |
|----------------------------|---------------------------------|-------------------------------------------------------|
| `SnapshotNotifierService`  | `Services/SnapshotNotifierService.cs` | Recebe dados e distribui para todos os clientes   |
| `ModbusPollingService`     | `Services/ModbusPollingService.cs`    | Lê o sensor em loop e envia para o notificador    |
| `BerthingController`       | `Controllers/BerthingController.cs`   | Endpoint SSE que o cliente consome                |
| `Program.cs`               | `Program.cs`                          | Registra tudo no container de injeção de dependência |

---

## 4. Peça 1 — O Notificador (SnapshotNotifierService)

**Arquivo:** `Services/SnapshotNotifierService.cs`

Este é o **coração** do sistema de streaming. Ele funciona como um **carteiro** que recebe uma carta (snapshot) e entrega uma cópia para cada destinatário (cliente SSE conectado).

### Código completo (com explicação linha a linha)

```csharp
using System.Threading.Channels;
using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Services;

public class SnapshotNotifierService
{
    // Lista de channels — cada cliente SSE conectado tem um channel próprio
    private readonly List<Channel<ModbusSnapshot>> _subscribers = new();

    // Lock para garantir que apenas uma thread mexe na lista por vez
    private readonly object _lock = new();
```

#### O que é um `Channel<T>`?

Um **Channel** é uma estrutura do C# para comunicação entre threads. Pense nele como um **cano**:

- Um lado do cano é o **Writer** (quem escreve = produtor).
- O outro lado é o **Reader** (quem lê = consumidor).
- O dado entra de um lado e sai do outro, de forma **segura entre threads**.

```
Writer ────► [ Channel (buffer) ] ────► Reader
```

No nosso caso:
- O `ModbusPollingService` escreve snapshots no channel (via `Notify()`).
- Cada endpoint SSE lê do seu channel (via `SubscribeAsync()`).

#### Método `Notify()` — Publicar para todos os clientes

```csharp
    public void Notify(ModbusSnapshot snapshot)
    {
        lock (_lock)
        {
            // Remove channels de clientes que já desconectaram
            _subscribers.RemoveAll(ch => ch.Reader.Completion.IsCompleted);

            // Envia uma cópia do snapshot para cada cliente conectado
            foreach (var channel in _subscribers)
            {
                // TryWrite: tenta escrever sem bloquear
                // Se o buffer estiver cheio, o snapshot antigo é descartado
                // (porque usamos BoundedChannelFullMode.DropOldest)
                channel.Writer.TryWrite(snapshot);
            }
        }
    }
```

**O que acontece aqui:**

1. `lock (_lock)` — Trava o acesso para que apenas uma thread mexa na lista por vez. Isso é necessário porque o `Notify()` é chamado pelo background service (uma thread) e o `SubscribeAsync()` é chamado por requisições HTTP (outras threads).
2. `RemoveAll(...)` — Limpa channels de clientes que já desconectaram, evitando vazamento de memória.
3. `TryWrite(snapshot)` — Envia o snapshot para cada cliente. O `Try` significa que, se não conseguir (buffer cheio), não trava — simplesmente ignora.

#### Método `SubscribeAsync()` — Criar uma assinatura para um cliente

```csharp
    public async IAsyncEnumerable<ModbusSnapshot> SubscribeAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken)
    {
        // Cria um channel com buffer de 1 (guarda apenas o último snapshot)
        var channel = Channel.CreateBounded<ModbusSnapshot>(
            new BoundedChannelOptions(1)
            {
                FullMode = BoundedChannelFullMode.DropOldest
            });

        // Adiciona este channel à lista de subscribers
        lock (_lock)
        {
            _subscribers.Add(channel);
        }

        try
        {
            // Fica lendo do channel para sempre, até o cliente desconectar
            await foreach (var snapshot in channel.Reader.ReadAllAsync(cancellationToken))
            {
                // Cada snapshot lido é "devolvido" para quem chamou SubscribeAsync()
                yield return snapshot;
            }
        }
        finally
        {
            // Quando o cliente desconecta, limpa o channel
            lock (_lock)
            {
                _subscribers.Remove(channel);
            }
            channel.Writer.TryComplete();
        }
    }
}
```

**O que acontece aqui:**

1. **Cria um channel com buffer de 1:** Se o cliente for lento para processar, o channel guarda apenas o snapshot mais recente e descarta os antigos. Isso evita consumo excessivo de memória.
2. **Adiciona à lista:** A partir deste momento, toda vez que `Notify()` for chamado, este channel receberá uma cópia do snapshot.
3. **`await foreach`:** Esse loop fica parado **esperando** até que um novo snapshot chegue no channel. Quando chega, o `yield return` o envia para quem chamou o método.
4. **`finally`:** Quando o cliente desconecta (ou ocorre cancelamento), o channel é removido da lista e fechado.

### Por que `IAsyncEnumerable`?

`IAsyncEnumerable<T>` é o equivalente assíncrono de `IEnumerable<T>`. Enquanto `IEnumerable` retorna itens um de cada vez **de forma síncrona**, `IAsyncEnumerable` retorna itens um de cada vez **aguardando assincronamente** até que o próximo item esteja disponível.

```csharp
// IEnumerable — todos os itens estão prontos na memória
foreach (var item in lista) { ... }

// IAsyncEnumerable — cada item pode demorar para chegar
await foreach (var item in streamDeItens) { ... }
```

Isso é perfeito para SSE porque os snapshots chegam ao longo do tempo (1 por segundo).

---

## 5. Peça 2 — O Produtor de Dados (ModbusPollingService)

**Arquivo:** `Services/ModbusPollingService.cs`

Este é um **Background Service** — um serviço que roda automaticamente em segundo plano quando a aplicação inicia. Sua função é ler o sensor Modbus em loop e alimentar o notificador.

### Trecho relevante para o streaming

```csharp
public class ModbusPollingService : BackgroundService
{
    private readonly IModbusReaderService _reader;
    private readonly SnapshotNotifierService _notifier;
    // ... outros campos

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // 1. Lê os dados do sensor
                var snapshot = await _reader.ReadAllAsync();

                // 2. Atualiza o cache em memória (para o endpoint GET /snapshot)
                BerthingController.UpdateSnapshot(snapshot);

                // 3. ★ ENVIA PARA TODOS OS CLIENTES SSE ★
                _notifier.Notify(snapshot);
            }
            catch (Exception ex)
            {
                // Em caso de erro, registra e tenta de novo no próximo ciclo
            }

            // Aguarda 1 segundo antes da próxima leitura
            await Task.Delay(settings.ReadIntervalMs, stoppingToken);
        }
    }
}
```

**O que importa para o streaming:**

A linha `_notifier.Notify(snapshot)` é o ponto onde o dado produzido pelo sensor é **injetado** no sistema de streaming. Sem essa linha, os clientes SSE ficariam esperando dados para sempre.

### O que é um `BackgroundService`?

É uma classe base do ASP.NET Core que roda uma tarefa em segundo plano. Você só precisa sobrescrever o método `ExecuteAsync()` e colocar seu loop lá dentro.

```csharp
public class MeuServico : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Faça algo útil aqui
            await Task.Delay(1000, stoppingToken); // espera 1 segundo
        }
    }
}
```

O ASP.NET Core automaticamente:
- Inicia o serviço quando a aplicação sobe.
- Para o serviço quando a aplicação é desligada (via `stoppingToken`).

---

## 6. Peça 3 — O Endpoint SSE (BerthingController)

**Arquivo:** `Controllers/BerthingController.cs`

Este é o endpoint que o cliente (Flutter, navegador, curl, etc.) chama para começar a receber dados em tempo real.

### Código completo do endpoint (com explicação)

```csharp
[HttpGet("stream")]
public async Task StreamSnapshot(CancellationToken cancellationToken)
{
    // ---- PASSO 1: Configurar os headers HTTP ----

    // Diz ao cliente que a resposta é um stream de eventos
    Response.ContentType = "text/event-stream";

    // Diz ao cliente (e proxies) para NÃO cachear os dados
    Response.Headers.CacheControl = "no-cache";

    // Informa que a conexão deve ser mantida aberta
    Response.Headers.Connection = "keep-alive";

    // ---- PASSO 2: Desabilitar buffering ----

    // Por padrão, o ASP.NET Core acumula dados em um buffer antes de enviar.
    // Para SSE, precisamos que cada Write chegue IMEDIATAMENTE ao cliente.
    HttpContext.Features
        .Get<Microsoft.AspNetCore.Http.Features.IHttpResponseBodyFeature>()
        ?.DisableBuffering();

    // ---- PASSO 3: Enviar os headers ----

    // Faz flush para que o cliente receba os headers imediatamente
    // e saiba que a conexão SSE foi estabelecida com sucesso.
    await Response.Body.FlushAsync(cancellationToken);

    // ---- PASSO 4: Loop de envio de dados ----

    try
    {
        // Cria uma assinatura no notificador e itera sobre os snapshots
        await foreach (var snapshot in _notifier.SubscribeAsync(cancellationToken))
        {
            // Serializa o snapshot para JSON
            var json = JsonSerializer.Serialize(snapshot);

            // Escreve no formato SSE: "data: {json}\n\n"
            await Response.WriteAsync($"data: {json}\n\n", cancellationToken);

            // Força o envio imediato ao cliente (sem esperar o buffer encher)
            await Response.Body.FlushAsync(cancellationToken);
        }
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
        // O cliente desconectou — isso é NORMAL em SSE.
        // Não é um erro, não precisa logar nem tratar.
    }
}
```

### Detalhes Importantes

#### Por que o retorno é `Task` e não `ActionResult`?

Em endpoints normais, retornamos `ActionResult<T>` ou `Ok(dados)`. Mas no SSE, **nós mesmos controlamos a resposta** escrevendo diretamente no `Response`. Não há um "retorno" — a resposta é um stream contínuo de dados.

#### Por que `DisableBuffering()`?

Sem isso, o ASP.NET Core pode acumular vários snapshots no buffer antes de enviar tudo de uma vez. O cliente ficaria sem receber dados por vários segundos, o que anula o propósito do tempo real.

#### Por que `FlushAsync()`?

O `Flush` força o envio imediato. Mesmo com o buffering desabilitado, é boa prática chamar `FlushAsync()` após cada escrita para garantir que o dado chegue ao cliente sem atraso.

#### O que é `CancellationToken`?

O `CancellationToken` é um mecanismo do C# para sinalizar que uma operação deve ser cancelada. No caso do SSE:

- Quando o **cliente desconecta** (fecha o app, perde internet), o ASP.NET Core automaticamente "cancela" o token.
- O `await foreach` detecta o cancelamento e para o loop.
- O `catch (OperationCanceledException)` captura essa exceção — que é **esperada e normal**.

---

## 7. Peça 4 — Registro no Program.cs

**Arquivo:** `Program.cs`

Para tudo funcionar, os serviços precisam ser **registrados** no container de injeção de dependência do ASP.NET Core.

```csharp
// Registra o notificador como Singleton (uma única instância compartilhada)
builder.Services.AddSingleton<SnapshotNotifierService>();

// Registra o serviço de polling como Hosted Service (roda em background)
builder.Services.AddHostedService<ModbusPollingService>();
```

### Por que `Singleton`?

O `SnapshotNotifierService` **precisa ser Singleton** porque:

- Todos os clientes SSE devem se inscrever **na mesma instância**.
- O `ModbusPollingService` deve enviar dados para **a mesma instância**.
- Se cada requisição criasse uma instância nova, os clientes nunca receberiam os dados.

| Lifetime  | Comportamento                        | Quando usar                                |
|-----------|--------------------------------------|--------------------------------------------|
| Singleton | Uma instância para toda a aplicação  | Serviços compartilhados (notificadores)    |
| Scoped    | Uma instância por requisição HTTP    | Repositórios, serviços de banco de dados   |
| Transient | Nova instância toda vez que é pedida | Serviços leves sem estado                  |

### Por que `AddHostedService`?

O `AddHostedService<T>()` diz ao ASP.NET Core: "Quando a aplicação iniciar, crie uma instância dessa classe e chame `ExecuteAsync()` automaticamente." É assim que o `ModbusPollingService` começa a rodar em background.

---

## 8. Fluxo Completo Passo a Passo

Agora que você conhece cada peça, veja o fluxo completo de quando um cliente se conecta até receber dados:

```
1. A aplicação inicia
   └── Program.cs registra SnapshotNotifierService (Singleton)
   └── Program.cs registra ModbusPollingService (HostedService)
       └── ExecuteAsync() começa a rodar em loop

2. O ModbusPollingService lê o sensor a cada 1 segundo
   └── _reader.ReadAllAsync() → retorna um ModbusSnapshot
   └── _notifier.Notify(snapshot) → mas ninguém está ouvindo (ainda)

3. Um cliente faz GET /api/berthing/stream
   └── O ASP.NET Core chama StreamSnapshot() no controller
       └── Configura os headers (text/event-stream, no-cache, keep-alive)
       └── Desabilita buffering
       └── Chama _notifier.SubscribeAsync()
           └── Cria um Channel<ModbusSnapshot> com buffer de 1
           └── Adiciona o channel à lista de subscribers
           └── Começa a esperar dados...

4. O ModbusPollingService faz a próxima leitura (1 segundo depois)
   └── _notifier.Notify(snapshot)
       └── Itera pela lista de subscribers
       └── Encontra o channel do cliente
       └── channel.Writer.TryWrite(snapshot) ← dado chega no channel

5. O SubscribeAsync() recebe o dado
   └── channel.Reader.ReadAllAsync() produz o snapshot
   └── yield return snapshot → entrega para o controller

6. O controller envia o dado ao cliente
   └── JsonSerializer.Serialize(snapshot) → converte para JSON
   └── Response.WriteAsync("data: {json}\n\n") → escreve na resposta
   └── Response.Body.FlushAsync() → garante envio imediato

7. O cliente recebe a linha:
   data: {"DataHoraLeitura":"2026-03-13T10:00:01Z","LifeCounter":42,...}

8. Repetir passos 4-7 para cada nova leitura do sensor.

9. O cliente desconecta
   └── CancellationToken é cancelado
   └── await foreach para de iterar
   └── finally: remove o channel da lista de subscribers
   └── O endpoint retorna normalmente
```

---

## 9. Conceitos-chave do C# usados

### `Channel<T>` (System.Threading.Channels)

Fila thread-safe para comunicação entre produtores e consumidores.

```csharp
// Criar um channel com buffer de 1 (descarta antigos se estiver cheio)
var channel = Channel.CreateBounded<string>(new BoundedChannelOptions(1)
{
    FullMode = BoundedChannelFullMode.DropOldest
});

// Escrever (produtor)
channel.Writer.TryWrite("Olá!");

// Ler (consumidor) — espera até ter algo disponível
await foreach (var item in channel.Reader.ReadAllAsync(cancellationToken))
{
    Console.WriteLine(item); // "Olá!"
}
```

### `IAsyncEnumerable<T>` + `yield return`

Produz itens sob demanda de forma assíncrona.

```csharp
async IAsyncEnumerable<int> ContarAteDez(
    [EnumeratorCancellation] CancellationToken ct)
{
    for (int i = 1; i <= 10; i++)
    {
        await Task.Delay(1000, ct); // espera 1 segundo
        yield return i;             // entrega o número para o consumidor
    }
}

// Consumir:
await foreach (var numero in ContarAteDez(cancellationToken))
{
    Console.WriteLine(numero); // 1, 2, 3, ... (um por segundo)
}
```

### `BackgroundService`

Classe base para tarefas que rodam em segundo plano por toda a vida da aplicação.

### `CancellationToken`

Mecanismo para cancelar operações assíncronas de forma cooperativa. O token é passado adiante e cada operação verifica se o cancelamento foi solicitado.

### `lock`

Garante que apenas uma thread execute um bloco de código por vez. Essencial quando múltiplas threads acessam dados compartilhados (a lista de subscribers, por exemplo).

---

## 10. Como criar do zero uma feature de streaming semelhante

Se você quiser criar uma funcionalidade de streaming para outro tipo de dado (por exemplo, alertas em tempo real), siga este passo a passo:

### Passo 1 — Criar o notificador (Pub/Sub)

Crie um serviço genérico que distribui dados para clientes conectados:

```csharp
// Services/AlertNotifierService.cs
using System.Threading.Channels;

public class AlertNotifierService
{
    private readonly List<Channel<MinhaAlerta>> _subscribers = new();
    private readonly object _lock = new();

    // Chamado por quem PRODUZ os dados
    public void Notify(MinhaAlerta alerta)
    {
        lock (_lock)
        {
            _subscribers.RemoveAll(ch => ch.Reader.Completion.IsCompleted);

            foreach (var ch in _subscribers)
            {
                ch.Writer.TryWrite(alerta);
            }
        }
    }

    // Chamado por quem CONSOME os dados (endpoint SSE)
    public async IAsyncEnumerable<MinhaAlerta> SubscribeAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken)
    {
        var channel = Channel.CreateBounded<MinhaAlerta>(
            new BoundedChannelOptions(1)
            {
                FullMode = BoundedChannelFullMode.DropOldest
            });

        lock (_lock) { _subscribers.Add(channel); }

        try
        {
            await foreach (var alerta in channel.Reader.ReadAllAsync(cancellationToken))
            {
                yield return alerta;
            }
        }
        finally
        {
            lock (_lock) { _subscribers.Remove(channel); }
            channel.Writer.TryComplete();
        }
    }
}
```

### Passo 2 — Criar o produtor de dados

Se os dados vêm de um processo periódico, use um `BackgroundService`:

```csharp
// Services/AlertPollingService.cs
public class AlertPollingService : BackgroundService
{
    private readonly AlertNotifierService _notifier;

    public AlertPollingService(AlertNotifierService notifier)
    {
        _notifier = notifier;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Substitua pela sua lógica de obtenção de dados
            var alerta = await VerificarAlertasAsync();

            if (alerta != null)
            {
                _notifier.Notify(alerta);
            }

            await Task.Delay(5000, stoppingToken); // verifica a cada 5 segundos
        }
    }
}
```

Se os dados vêm de uma ação do usuário (por exemplo, uma mensagem de chat), chame `_notifier.Notify()` diretamente de dentro do controller ou service que recebe a ação.

### Passo 3 — Criar o endpoint SSE

```csharp
// Controllers/AlertController.cs
[ApiController]
[Route("api/[controller]")]
public class AlertController : ControllerBase
{
    private readonly AlertNotifierService _notifier;

    public AlertController(AlertNotifierService notifier)
    {
        _notifier = notifier;
    }

    [HttpGet("stream")]
    public async Task StreamAlertas(CancellationToken cancellationToken)
    {
        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.Connection = "keep-alive";

        HttpContext.Features
            .Get<Microsoft.AspNetCore.Http.Features.IHttpResponseBodyFeature>()
            ?.DisableBuffering();

        await Response.Body.FlushAsync(cancellationToken);

        try
        {
            await foreach (var alerta in _notifier.SubscribeAsync(cancellationToken))
            {
                var json = JsonSerializer.Serialize(alerta);
                await Response.WriteAsync($"data: {json}\n\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Cliente desconectou — normal.
        }
    }
}
```

### Passo 4 — Registrar no Program.cs

```csharp
// O notificador DEVE ser Singleton
builder.Services.AddSingleton<AlertNotifierService>();

// Se você tem um background service, registre como HostedService
builder.Services.AddHostedService<AlertPollingService>();
```

### Checklist final

- [ ] Notificador registrado como **Singleton**
- [ ] Produtor chamando `Notify()` quando há dado novo
- [ ] Endpoint com `Content-Type: text/event-stream`
- [ ] Buffering desabilitado com `DisableBuffering()`
- [ ] `FlushAsync()` após cada `WriteAsync()`
- [ ] Formato SSE correto: `data: {json}\n\n`
- [ ] `CancellationToken` passado para todas as operações assíncronas
- [ ] `OperationCanceledException` tratada no endpoint

---

## 11. Testando o Endpoint SSE

### Com curl (terminal)

```bash
curl -N \
  -H "Authorization: Bearer SEU_TOKEN" \
  -H "Accept: text/event-stream" \
  http://127.0.0.1:5000/api/Berthing/stream
```

O `-N` desabilita o buffering do curl para que os dados apareçam em tempo real.

### Com PowerShell

```powershell
curl -N `
  -H "Authorization: Bearer SEU_TOKEN" `
  -H "Accept: text/event-stream" `
  http://127.0.0.1:5000/api/Berthing/stream
```

### Saída esperada

```
data: {"DataHoraLeitura":"2026-03-13T10:00:01Z","LifeCounter":1,...}

data: {"DataHoraLeitura":"2026-03-13T10:00:02Z","LifeCounter":2,...}

data: {"DataHoraLeitura":"2026-03-13T10:00:03Z","LifeCounter":3,...}
```

Cada linha aparece a cada ~1 segundo. Pressione `Ctrl+C` para desconectar.

---

## 12. Erros Comuns e Troubleshooting

### "Não recebo nenhum dado"

- Verifique se o `ModbusPollingService` está rodando (olhe os logs).
- Confira se o `SnapshotNotifierService` está registrado como **Singleton** (não Scoped/Transient).
- Verifique se o token JWT não expirou.

### "Dados chegam com atraso (em lotes)"

- Verifique se `DisableBuffering()` está sendo chamado.
- Verifique se `FlushAsync()` está sendo chamado após cada `WriteAsync()`.
- Proxies reversos (Nginx, IIS) podem bufferizar — configure-os para não cachear respostas SSE.

### "A conexão cai depois de alguns minutos"

- Load balancers podem fechar conexões ociosas. Configure o timeout para um valor alto.
- Implemente reconexão automática no cliente.

### "Erro 401 Unauthorized"

- O endpoint requer autenticação JWT. Inclua o header `Authorization: Bearer {token}`.
- Verifique se o token não expirou.

### "Exceção OperationCanceledException nos logs"

- Se for dentro do endpoint SSE, é **normal** — significa que o cliente desconectou.
- O `catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)` evita que isso vire um erro 500.

---

> **Dica:** Se você entendeu este documento, já tem conhecimento suficiente para implementar streaming SSE em qualquer projeto ASP.NET Core. A estrutura é sempre a mesma: **Notificador (Pub/Sub) + Produtor + Endpoint SSE**.
