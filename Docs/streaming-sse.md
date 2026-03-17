# Consumindo Dados em Tempo Real via Stream (SSE)

## Índice
1. [Visão Geral](#visão-geral)
2. [Como Funciona o SSE](#como-funciona-o-sse)
3. [Endpoint da API](#endpoint-da-api)
4. [Formato dos Dados](#formato-dos-dados)
5. [Implementação no Flutter](#implementação-no-flutter)
6. [Tratamento de Reconexão](#tratamento-de-reconexão)
7. [Exemplo Completo com UI](#exemplo-completo-com-ui)
8. [Troubleshooting](#troubleshooting)

---

## Visão Geral

A API expõe um endpoint **SSE (Server-Sent Events)** que envia os dados do sensor Modbus em tempo real. Com isso, o Flutter mantém uma conexão aberta e recebe automaticamente cada novo snapshot assim que ele é gerado (a cada ~1 segundo), sem precisar fazer polling manual.

### Vantagens

- **Performance** — A tela só é atualizada quando há dados novos (sem requisições desnecessárias).
- **Menor latência** — Os dados chegam assim que são lidos do sensor.
- **Simples** — SSE é HTTP padrão; funciona com qualquer cliente HTTP.

---

## Como Funciona o SSE

```
Flutter (Client)                         API (Server)
     |                                        |
     |  GET /api/berthing/stream              |
     |  Authorization: Bearer {token}         |
     |--------------------------------------->|
     |                                        |
     |  Content-Type: text/event-stream       |
     |<---------------------------------------|
     |                                        |
     |  data: {"DataHoraLeitura":...}         |  <- snapshot 1
     |<---------------------------------------|
     |                                        |
     |  data: {"DataHoraLeitura":...}         |  <- snapshot 2
     |<---------------------------------------|
     |                                        |
     |  data: {"DataHoraLeitura":...}         |  <- snapshot 3
     |<---------------------------------------|
     |           ... continua ...             |
```

A conexão fica aberta indefinidamente. O servidor envia uma linha `data: {json}\n\n` a cada novo snapshot.

---

## Endpoint da API

```
GET /api/berthing/stream
```

**Headers obrigatórios:**
| Header          | Valor                  |
|-----------------|------------------------|
| Authorization   | `Bearer {seu_token}`   |

**Resposta:**
- Content-Type: `text/event-stream`
- Cache-Control: `no-cache`
- Connection: `keep-alive`

Cada evento é enviado no formato SSE padrão:
```
data: {"DataHoraLeitura":"2026-03-12T14:30:00Z","LifeCounter":1234,...}\n\n
```

---

## Formato dos Dados

Cada evento contém um JSON com a estrutura `ModbusSnapshot`:

```json
{
  "DataHoraLeitura": "2026-03-12T14:30:00Z",
  "LifeCounter": 1234,
  "ComunicacaoAtiva": true,
  "Berco1": {
    "BercoId": "B1",
    "StatusBas": { ... },
    "StatusLdd": { ... },
    "FarRangeWarning": 15.0,
    "FarRangeAlarm": 20.0,
    ...
  },
  "Berco2": {
    "BercoId": "B2",
    ...
  }
}
```

---

## Implementação no Flutter

### 1. Adicionar dependência

No `pubspec.yaml`:

```yaml
dependencies:
  dio: ^5.0.0
```

### 2. Criar o serviço de stream

```dart
import 'dart:async';
import 'dart:convert';
import 'package:dio/dio.dart';

class BerthingStreamService {
  final String baseUrl;
  final String Function() getToken;
  final Dio _dio;

  BerthingStreamService({
    required this.baseUrl,
    required this.getToken,
    Dio? dio,
  }) : _dio = dio ?? Dio();

  /// Conecta ao endpoint SSE e retorna um Stream de snapshots.
  /// Cada evento emitido é um Map<String, dynamic> com os dados do sensor.
  Stream<Map<String, dynamic>> connectToSnapshotStream() async* {
    final response = await _dio.get<ResponseBody>(
      '$baseUrl/api/berthing/stream',
      options: Options(
        headers: {
          'Authorization': 'Bearer ${getToken()}',
          'Accept': 'text/event-stream',
        },
        responseType: ResponseType.stream,
      ),
    );

    if (response.statusCode != 200) {
      throw Exception('Falha ao conectar ao stream: ${response.statusCode}');
    }

    // Lê o stream de bytes e converte para linhas de texto
    await for (final chunk in response.data!.stream.transform(utf8.decoder)) {
      // Cada evento SSE vem no formato "data: {json}\n\n"
      for (final line in chunk.split('\n')) {
        if (line.startsWith('data: ')) {
          final jsonStr = line.substring(6); // Remove o prefixo "data: "
          final snapshot = jsonDecode(jsonStr) as Map<String, dynamic>;
          yield snapshot;
        }
      }
    }
  }
}
```

### 3. Usar na tela com StreamBuilder

```dart
import 'package:flutter/material.dart';

class BerthingScreen extends StatefulWidget {
  const BerthingScreen({super.key});

  @override
  State<BerthingScreen> createState() => _BerthingScreenState();
}

class _BerthingScreenState extends State<BerthingScreen> {
  late final BerthingStreamService _streamService;
  Stream<Map<String, dynamic>>? _snapshotStream;

  @override
  void initState() {
    super.initState();
    _streamService = BerthingStreamService(
      baseUrl: 'http://SEU_IP:PORTA',
      getToken: () => 'SEU_TOKEN_JWT_AQUI', // Substitua pela lógica real de token
    );
    _snapshotStream = _streamService.connectToSnapshotStream();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Dados em Tempo Real')),
      body: StreamBuilder<Map<String, dynamic>>(
        stream: _snapshotStream,
        builder: (context, snapshot) {
          if (snapshot.hasError) {
            return Center(child: Text('Erro: ${snapshot.error}'));
          }

          if (!snapshot.hasData) {
            return const Center(child: CircularProgressIndicator());
          }

          final data = snapshot.data!;
          final comunicacaoAtiva = data['ComunicacaoAtiva'] as bool;
          final lifeCounter = data['LifeCounter'] as int;
          final berco1 = data['Berco1'] as Map<String, dynamic>;
          final berco2 = data['Berco2'] as Map<String, dynamic>;

          return Padding(
            padding: const EdgeInsets.all(16.0),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  children: [
                    Icon(
                      comunicacaoAtiva ? Icons.wifi : Icons.wifi_off,
                      color: comunicacaoAtiva ? Colors.green : Colors.red,
                    ),
                    const SizedBox(width: 8),
                    Text(comunicacaoAtiva ? 'Conectado' : 'Desconectado'),
                  ],
                ),
                const SizedBox(height: 8),
                Text('Life Counter: $lifeCounter'),
                const Divider(),
                Text('Berço 1: ${berco1['BercoId']}',
                    style: const TextStyle(fontWeight: FontWeight.bold)),
                // ... exibir dados do Berço 1
                const Divider(),
                Text('Berço 2: ${berco2['BercoId']}',
                    style: const TextStyle(fontWeight: FontWeight.bold)),
                // ... exibir dados do Berço 2
              ],
            ),
          );
        },
      ),
    );
  }
}
```

---

## Tratamento de Reconexão

Em produção, a conexão SSE pode ser interrompida (rede instável, servidor reiniciado, etc.). É importante implementar reconexão automática:

```dart
class BerthingStreamService {
  // ... campos anteriores ...

  /// Stream com reconexão automática.
  /// Se a conexão cair, tenta reconectar após [retryDelay].
  Stream<Map<String, dynamic>> connectWithRetry({
    Duration retryDelay = const Duration(seconds: 3),
  }) async* {
    while (true) {
      try {
        await for (final snapshot in connectToSnapshotStream()) {
          yield snapshot;
        }
      } catch (e) {
        debugPrint('Conexão SSE perdida: $e. Reconectando em ${retryDelay.inSeconds}s...');
        await Future.delayed(retryDelay);
      }
    }
  }
}
```

Para usar com reconexão:

```dart
_snapshotStream = _streamService.connectWithRetry();
```

---

## Exemplo Completo com UI

Abaixo está um exemplo completo de um widget que consome o stream com reconexão e exibe o status em tempo real:

```dart
class BerthingRealtimeWidget extends StatefulWidget {
  final BerthingStreamService streamService;
  const BerthingRealtimeWidget({super.key, required this.streamService});

  @override
  State<BerthingRealtimeWidget> createState() => _BerthingRealtimeWidgetState();
}

class _BerthingRealtimeWidgetState extends State<BerthingRealtimeWidget> {
  late final Stream<Map<String, dynamic>> _stream;

  @override
  void initState() {
    super.initState();
    // Usa o stream com reconexão automática
    _stream = widget.streamService.connectWithRetry();
  }

  @override
  Widget build(BuildContext context) {
    return StreamBuilder<Map<String, dynamic>>(
      stream: _stream,
      builder: (context, snapshot) {
        if (snapshot.connectionState == ConnectionState.waiting) {
          return const Center(child: CircularProgressIndicator());
        }

        if (snapshot.hasError) {
          return Center(
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                const Icon(Icons.error_outline, size: 48, color: Colors.red),
                const SizedBox(height: 8),
                Text('Erro: ${snapshot.error}'),
              ],
            ),
          );
        }

        final data = snapshot.data!;
        return Card(
          child: ListTile(
            leading: Icon(
              data['ComunicacaoAtiva'] == true ? Icons.sensors : Icons.sensors_off,
              color: data['ComunicacaoAtiva'] == true ? Colors.green : Colors.red,
            ),
            title: Text('Life Counter: ${data['LifeCounter']}'),
            subtitle: Text('Última leitura: ${data['DataHoraLeitura']}'),
          ),
        );
      },
    );
  }
}
```

---

## Troubleshooting

| Problema | Causa | Solução |
|----------|-------|---------|
| `401 Unauthorized` | Token JWT expirado ou ausente | Renovar o token antes de conectar ao stream |
| Stream não recebe dados | Servidor não iniciou o polling | Verificar logs do backend — o `ModbusPollingService` deve estar rodando |
| Conexão fecha imediatamente | Proxy ou load balancer cortando conexões longas | Configurar timeout do proxy para conexões SSE |
| Dados atrasados | Cliente processando lento | O servidor descarta snapshots antigos automaticamente (buffer de 1) |
| `Connection refused` | API não está rodando ou IP/porta incorretos | Verificar se a API está acessível no IP e porta corretos |
