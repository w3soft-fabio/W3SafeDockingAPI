namespace WebSafeDockingAPI.Services;

/// <summary>
/// Implementação fake do serviço de conexão Modbus para desenvolvimento.
/// Simula um cenário realista de atracação com dados dinâmicos.
///
/// CICLO DE SIMULAÇÃO:
/// 1. APPROACHING  (~180 ticks)  — Navio parte de ~300m e se aproxima até ~0m
/// 2. DOCKED       (~60  ticks)  — Navio atracado, pequenas oscilações (drifting)
/// 3. DEPARTING    (~90  ticks)  — Navio se afasta até ~300m
/// 4. IDLE         (~30  ticks)  — Pausa antes do próximo ciclo
/// → Loop infinito
///
/// Cada tick = 1 chamada de ReadHoldingRegistersAsync (~1 segundo).
/// O Berço 2 simula um navio em fase diferente (defasado 180 ticks).
/// </summary>
public class FakeModbusConnectionService : IModbusConnectionService
{
    private ushort _lifeCounter = 0;
    private int _tick = 0;
    private readonly Random _rng = new(42); // seed fixa para reprodutibilidade

    // ---- Durações de cada fase (em ticks / ~segundos) ----
    private const int APPROACHING_TICKS = 180;
    private const int DOCKED_TICKS = 60;
    private const int DEPARTING_TICKS = 90;
    private const int IDLE_TICKS = 30;
    private const int TOTAL_CYCLE = APPROACHING_TICKS + DOCKED_TICKS + DEPARTING_TICKS + IDLE_TICKS; // 360

    // ---- Parâmetros de simulação ----
    private const double MAX_DISTANCE_M = 300.0;   // Distância inicial (metros)
    private const double DOCKED_DISTANCE_M = 1.5;  // Distância quando atracado
    private const double MAX_SPEED_CMS = 25.0;     // Velocidade máxima (cm/s) — início da aproximação
    private const double SENSOR_OFFSET_M = 0.5;    // Diferença entre S1 e S2 para gerar ângulo

    // ---- Thresholds (valores brutos — serão ×0.1 pelo ReaderService) ----
    private const ushort FAR_WARNING = 150;    // 15.0 cm/s
    private const ushort FAR_ALARM = 200;      // 20.0 cm/s
    private const ushort MID_WARNING = 100;    // 10.0 cm/s
    private const ushort MID_ALARM = 150;      // 15.0 cm/s
    private const ushort CLOSE_WARNING = 50;   //  5.0 cm/s
    private const ushort CLOSE_ALARM = 80;     //  8.0 cm/s
    private const ushort MAX_ANGLE = 50;       //  5.0°
    private const ushort DRIFT_WARNING = 20;   //  2.0 m
    private const ushort DRIFT_ALARM = 50;     //  5.0 m

    public bool IsConnected => true;

    public Task<ushort[]> ReadHoldingRegistersAsync(
        ushort startAddress, ushort numberOfRegisters)
    {
        var registers = new ushort[numberOfRegisters];

        // Life Counter — incrementa a cada leitura, reseta em 65535
        registers[0] = _lifeCounter++;

        // Berço 1 — posição atual no ciclo
        FillBerthRegisters(registers, berthOffset: 1, cycleTick: _tick);

        // Berço 2 — defasado meio ciclo para mostrar cenário diferente
        FillBerthRegisters(registers, berthOffset: 19, cycleTick: _tick + TOTAL_CYCLE / 2);

        _tick++;

        return Task.FromResult(registers);
    }

    /// <summary>
    /// Preenche os 18 registradores de um berço com dados simulados.
    /// </summary>
    private void FillBerthRegisters(ushort[] registers, int berthOffset, int cycleTick)
    {
        int phase = cycleTick % TOTAL_CYCLE;

        // ---- Calcular distância e velocidade com base na fase ----
        double distanceM;   // distância consolidada (metros)
        double speedCmS;    // velocidade (cm/s)
        bool isBerthing;
        bool isDrifting;

        if (phase < APPROACHING_TICKS)
        {
            // APPROACHING: distância diminui de MAX_DISTANCE_M até DOCKED_DISTANCE_M
            // Usa curva quadrática — desacelera conforme se aproxima
            double progress = (double)phase / APPROACHING_TICKS; // 0.0 → 1.0
            double easedProgress = 1.0 - Math.Pow(1.0 - progress, 2); // ease-out quadrático
            distanceM = MAX_DISTANCE_M - (MAX_DISTANCE_M - DOCKED_DISTANCE_M) * easedProgress;

            // Velocidade: alta no início, baixa perto do berço
            speedCmS = MAX_SPEED_CMS * (1.0 - easedProgress) + 0.5;

            isBerthing = true;
            isDrifting = false;
        }
        else if (phase < APPROACHING_TICKS + DOCKED_TICKS)
        {
            // DOCKED: navio parado com pequenas oscilações
            int dockedTick = phase - APPROACHING_TICKS;
            double drift = Math.Sin(dockedTick * 0.3) * 0.8; // oscilação ±0.8m
            distanceM = DOCKED_DISTANCE_M + drift;
            speedCmS = Math.Abs(Math.Cos(dockedTick * 0.3) * 0.5); // velocidade quase zero

            isBerthing = false;
            isDrifting = true;
        }
        else if (phase < APPROACHING_TICKS + DOCKED_TICKS + DEPARTING_TICKS)
        {
            // DEPARTING: distância aumenta de DOCKED_DISTANCE_M até MAX_DISTANCE_M
            int departTick = phase - APPROACHING_TICKS - DOCKED_TICKS;
            double progress = (double)departTick / DEPARTING_TICKS;
            double easedProgress = Math.Pow(progress, 2); // ease-in quadrático — acelera saindo
            distanceM = DOCKED_DISTANCE_M + (MAX_DISTANCE_M - DOCKED_DISTANCE_M) * easedProgress;
            speedCmS = MAX_SPEED_CMS * easedProgress + 0.3;

            isBerthing = false;
            isDrifting = false;
        }
        else
        {
            // IDLE: navio longe, sem movimento
            distanceM = MAX_DISTANCE_M;
            speedCmS = 0.0;
            isBerthing = false;
            isDrifting = false;
        }

        // ---- Adicionar ruído realista (±2%) ----
        double noise1 = 1.0 + (_rng.NextDouble() - 0.5) * 0.04;
        double noise2 = 1.0 + (_rng.NextDouble() - 0.5) * 0.04;
        double noiseSpeed1 = 1.0 + (_rng.NextDouble() - 0.5) * 0.06;
        double noiseSpeed2 = 1.0 + (_rng.NextDouble() - 0.5) * 0.06;

        // S1 e S2 com leve diferença para gerar ângulo
        double angleOffset = Math.Sin(_tick * 0.05) * SENSOR_OFFSET_M;
        double s1DistM = distanceM + angleOffset * 0.5;
        double s2DistM = distanceM - angleOffset * 0.5;

        s1DistM *= noise1;
        s2DistM *= noise2;

        double s1Speed = speedCmS * noiseSpeed1;
        double s2Speed = speedCmS * noiseSpeed2;

        double consolidatedDistM = (s1DistM + s2DistM) / 2.0;
        double angleDeg = Math.Atan2(s1DistM - s2DistM, 25.0) * (180.0 / Math.PI); // 25m entre sensores

        // LDD intensity — maior quando mais perto, com ruído
        double lddIntensity = Math.Clamp(95.0 - (consolidatedDistM / MAX_DISTANCE_M) * 40.0
            + (_rng.NextDouble() - 0.5) * 5.0, 20.0, 100.0);

        // ---- Converter para valores brutos de registrador (ushort) ----
        // Distância: valor_real / 0.1 = valor_bruto  → signed
        short s1DistRaw = (short)Math.Round(s1DistM / 0.1);
        short s2DistRaw = (short)Math.Round(s2DistM / 0.1);
        short distRaw = (short)Math.Round(consolidatedDistM / 0.1);
        short angleRaw = (short)Math.Round(angleDeg / 0.1);

        // Velocidade: valor_real / 0.01 = valor_bruto → unsigned
        ushort s1SpeedRaw = (ushort)Math.Clamp(Math.Round(s1Speed / 0.01), 0, 65535);
        ushort s2SpeedRaw = (ushort)Math.Clamp(Math.Round(s2Speed / 0.01), 0, 65535);

        // LDD: valor_real / 0.1 = valor_bruto → unsigned
        ushort lddRaw = (ushort)Math.Clamp(Math.Round(lddIntensity / 0.1), 0, 1000);

        // ---- Calcular BAS Status bitmask ----
        ushort basStatus = ComputeBasStatus(
            isBerthing, isDrifting,
            speedCmS, consolidatedDistM,
            isDrifting && Math.Abs(distanceM - DOCKED_DISTANCE_M) > DRIFT_WARNING * 0.1,
            isDrifting && Math.Abs(distanceM - DOCKED_DISTANCE_M) > DRIFT_ALARM * 0.1);

        // LDD Status — tudo ok na simulação
        ushort lddStatus = 0;

        // ---- Preencher registradores ----
        registers[berthOffset + 0] = basStatus;

        // Thresholds (fixos)
        registers[berthOffset + 1] = FAR_WARNING;
        registers[berthOffset + 2] = FAR_ALARM;
        registers[berthOffset + 3] = MID_WARNING;
        registers[berthOffset + 4] = MID_ALARM;
        registers[berthOffset + 5] = CLOSE_WARNING;
        registers[berthOffset + 6] = CLOSE_ALARM;

        // Angle & Drifting thresholds
        registers[berthOffset + 7] = MAX_ANGLE;
        registers[berthOffset + 8] = DRIFT_WARNING;
        registers[berthOffset + 9] = DRIFT_ALARM;

        // Sensor 1
        registers[berthOffset + 10] = unchecked((ushort)s1DistRaw);
        registers[berthOffset + 11] = s1SpeedRaw;

        // Sensor 2
        registers[berthOffset + 12] = unchecked((ushort)s2DistRaw);
        registers[berthOffset + 13] = s2SpeedRaw;

        // Consolidado
        registers[berthOffset + 14] = unchecked((ushort)distRaw);
        registers[berthOffset + 15] = unchecked((ushort)angleRaw);

        // LDD
        registers[berthOffset + 16] = lddStatus;
        registers[berthOffset + 17] = lddRaw;
    }

    /// <summary>
    /// Calcula o bitmask de status do BAS com base no estado atual da simulação.
    /// </summary>
    private ushort ComputeBasStatus(
        bool isBerthing, bool isDrifting,
        double speedCmS, double distanceM,
        bool driftWarning, bool driftAlarm)
    {
        int bits = 0;

        // Bit 1: Berthing mode
        if (isBerthing) bits |= (1 << 1);

        // Bit 2: Drifting mode
        if (isDrifting) bits |= (1 << 2);

        // Determinar faixa de distância
        bool isFar = distanceM > 100.0;
        bool isMid = distanceM > 30.0 && distanceM <= 100.0;
        bool isClose = distanceM <= 30.0;

        // Thresholds efetivos (convertidos de bruto para real: ×0.1)
        double warnThreshold = isFar ? FAR_WARNING * 0.1 : isMid ? MID_WARNING * 0.1 : CLOSE_WARNING * 0.1;
        double alarmThreshold = isFar ? FAR_ALARM * 0.1 : isMid ? MID_ALARM * 0.1 : CLOSE_ALARM * 0.1;

        // Side 1 speed flags (bits 3-5)
        if (speedCmS <= warnThreshold)
            bits |= (1 << 3); // Speed Ok
        else if (speedCmS <= alarmThreshold)
            bits |= (1 << 4); // Speed Warning
        else
        {
            bits |= (1 << 5); // Speed Alarm
            bits |= (1 << 0); // Alarm Geral
        }

        // Side 1 drifting flags (bits 6-7)
        if (driftWarning) bits |= (1 << 6);
        if (driftAlarm)
        {
            bits |= (1 << 7);
            bits |= (1 << 0); // Alarm Geral
        }

        // Side 2 — mirrors Side 1 (same ship, symmetric sensors)
        if (speedCmS <= warnThreshold)
            bits |= (1 << 8); // Speed Ok
        else if (speedCmS <= alarmThreshold)
            bits |= (1 << 9); // Speed Warning
        else
            bits |= (1 << 10); // Speed Alarm

        if (driftWarning) bits |= (1 << 11);
        if (driftAlarm) bits |= (1 << 12);

        return (ushort)bits;
    }
}
