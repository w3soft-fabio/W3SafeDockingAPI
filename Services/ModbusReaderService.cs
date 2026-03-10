using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Services;

/// <summary>
/// Serviço que lê os registradores Modbus e converte em objetos organizados.
///
/// FLUXO:
/// 1. Lê os 37 registradores brutos (1000 a 1036)
/// 2. Extrai os valores de cada campo usando o índice correto
/// 3. Aplica fator de escala quando necessário
/// 4. Decodifica os campos de status (bitmasks)
/// 5. Monta e retorna o ModbusSnapshot completo
/// </summary>
public class ModbusReaderService : IModbusReaderService
{
    private readonly IModbusConnectionService _connection;
    private readonly ILogger<ModbusReaderService> _logger;

    // Endereço inicial dos registradores
    private const ushort START_ADDRESS = 1000;

    // Quantidade total de registradores a ler
    private const ushort REGISTER_COUNT = 37;

    public ModbusReaderService(
        IModbusConnectionService connection,
        ILogger<ModbusReaderService> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task<ModbusSnapshot> ReadAllAsync()
    {
        // PASSO 1: Lê todos os 37 registradores de uma vez
        ushort[] registers = await _connection.ReadHoldingRegistersAsync(
            START_ADDRESS,
            REGISTER_COUNT);

        // PASSO 2: Monta o snapshot
        var snapshot = new ModbusSnapshot
        {
            DataHoraLeitura = DateTime.UtcNow,
            LifeCounter = registers[0],   // Índice 0 = Endereço 1000
            ComunicacaoAtiva = true,       // Se chegou até aqui, a comunicação está ok
            Berco1 = ParseBerthData("B1", registers, offset: 1),   // Índice 1 = Endereço 1001
            Berco2 = ParseBerthData("B2", registers, offset: 19),  // Índice 19 = Endereço 1019
        };

        _logger.LogDebug(
            "Leitura Modbus OK — LifeCounter: {LC}, B1 Distance: {D1}m, B2 Distance: {D2}m",
            snapshot.LifeCounter,
            snapshot.Berco1.DistanciaConsolidadaMetros,
            snapshot.Berco2.DistanciaConsolidadaMetros);

        return snapshot;
    }

    /// <summary>
    /// Extrai os dados de UM berço a partir do array de registradores.
    ///
    /// O 'offset' indica onde começam os dados deste berço no array:
    ///   - B1: offset = 1  (registrador 1001)
    ///   - B2: offset = 19 (registrador 1019)
    ///
    /// Dentro de cada berço, a estrutura é IDÊNTICA (18 registradores):
    ///   [+0]  BAS Status
    ///   [+1]  Far Range Warning
    ///   [+2]  Far Range Alarm
    ///   [+3]  Middle Range Warning
    ///   [+4]  Middle Range Alarm
    ///   [+5]  Close Range Warning
    ///   [+6]  Close Range Alarm
    ///   [+7]  Max Ship Angle
    ///   [+8]  Drifting Warning
    ///   [+9]  Drifting Alarm
    ///   [+10] S1 Ship Distance     (SIGNED)
    ///   [+11] S1 Ship Speed
    ///   [+12] S2 Ship Distance     (SIGNED)
    ///   [+13] S2 Ship Speed
    ///   [+14] Ship Distance        (SIGNED)
    ///   [+15] Ship Angle           (SIGNED)
    ///   [+16] LDD Status
    ///   [+17] LDD Intensity
    /// </summary>
    private static BerthData ParseBerthData(string bercoId, ushort[] registers, int offset)
    {
        return new BerthData
        {
            BercoId = bercoId,

            // Status (bitmasks — sem fator de escala)
            StatusBas = DecodificarBasStatus(registers[offset + 0]),
            StatusLdd = DecodificarLddStatus(registers[offset + 16]),

            // Limiares de Velocidade (fator 0.1)
            FarRangeWarning    = registers[offset + 1] * 0.1,
            FarRangeAlarm      = registers[offset + 2] * 0.1,
            MiddleRangeWarning = registers[offset + 3] * 0.1,
            MiddleRangeAlarm   = registers[offset + 4] * 0.1,
            CloseRangeWarning  = registers[offset + 5] * 0.1,
            CloseRangeAlarm    = registers[offset + 6] * 0.1,

            // Ângulo Máximo (fator 0.1)
            AnguloMaximo = registers[offset + 7] * 0.1,

            // Limiares de Drifting (fator 0.1)
            DriftingWarning = registers[offset + 8] * 0.1,
            DriftingAlarm   = registers[offset + 9] * 0.1,

            // Sensor 1 — distância é SIGNED, velocidade é UNSIGNED
            Sensor1 = new SensorReading
            {
                DistanciaMetros        = (short)registers[offset + 10] * 0.1,
                VelocidadeCmPorSegundo = registers[offset + 11] * 0.01,
            },

            // Sensor 2 — mesma lógica
            Sensor2 = new SensorReading
            {
                DistanciaMetros        = (short)registers[offset + 12] * 0.1,
                VelocidadeCmPorSegundo = registers[offset + 13] * 0.01,
            },

            // Valores Consolidados — ambos SIGNED
            DistanciaConsolidadaMetros = (short)registers[offset + 14] * 0.1,
            AnguloNavio                = (short)registers[offset + 15] * 0.1,

            // Intensidade LDD (fator 0.1)
            IntensidadeLdd = registers[offset + 17] * 0.1,
        };
    }

    // ---- Métodos auxiliares de decodificação de bitmask ----

    private static bool IsBitSet(int value, int bitIndex)
    {
        return ((value >> bitIndex) & 1) == 1;
    }

    private static BasStatus DecodificarBasStatus(ushort rawValue)
    {
        return new BasStatus
        {
            AlarmGeral             = IsBitSet(rawValue, 0),
            ModoBerthingAtivo      = IsBitSet(rawValue, 1),
            ModoDriftingAtivo      = IsBitSet(rawValue, 2),
            Side1VelocidadeOk      = IsBitSet(rawValue, 3),
            Side1VelocidadeWarning = IsBitSet(rawValue, 4),
            Side1VelocidadeAlarm   = IsBitSet(rawValue, 5),
            Side1DriftingWarning   = IsBitSet(rawValue, 6),
            Side1DriftingAlarm     = IsBitSet(rawValue, 7),
            Side2VelocidadeOk      = IsBitSet(rawValue, 8),
            Side2VelocidadeWarning = IsBitSet(rawValue, 9),
            Side2VelocidadeAlarm   = IsBitSet(rawValue, 10),
            Side2DriftingWarning   = IsBitSet(rawValue, 11),
            Side2DriftingAlarm     = IsBitSet(rawValue, 12),
        };
    }

    private static LddStatus DecodificarLddStatus(ushort rawValue)
    {
        return new LddStatus
        {
            AlarmGeral       = IsBitSet(rawValue, 0),
            AlarmComunicacao = IsBitSet(rawValue, 1),
        };
    }
}
