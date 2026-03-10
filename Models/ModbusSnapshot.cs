namespace WebSafeDockingAPI.Models;

/// <summary>
/// Representa uma "foto" (snapshot) de todos os dados lidos do sensor
/// em um determinado instante.
/// </summary>
public class ModbusSnapshot
{
    /// <summary>
    /// Data e hora da leitura.
    /// </summary>
    public DateTime DataHoraLeitura { get; set; }

    /// <summary>
    /// Contador cíclico (0-65535) que incrementa 1 por segundo.
    /// Se parar de mudar = comunicação perdida.
    /// </summary>
    public int LifeCounter { get; set; }

    /// <summary>
    /// Indica se a comunicação com o sensor está ativa.
    /// </summary>
    public bool ComunicacaoAtiva { get; set; }

    /// <summary>
    /// Dados do Berço 1.
    /// </summary>
    public BerthData Berco1 { get; set; } = new();

    /// <summary>
    /// Dados do Berço 2.
    /// </summary>
    public BerthData Berco2 { get; set; } = new();
}
