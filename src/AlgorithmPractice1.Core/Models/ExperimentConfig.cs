using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AlgorithmPractice1.Core.Models;

/// <summary>
/// Параметры одного экспериментального прогона алгоритма.
/// </summary>
public record ExperimentConfig
{
    /// <summary>
    /// Максимальный размер входных данных N.
    /// </summary>
    public int NMax { get; init; } = 1000;

    /// <summary>
    /// Шаг изменения N.
    /// </summary>
    public int NStep { get; init; } = 100;

    /// <summary>
    /// Количество независимых прогонов на каждое N (по умолчанию 5).
    /// </summary>
    public int RunsPerN { get; init; } = 5;

    /// <summary>
    /// Второе измерение (M) для матричных алгоритмов (A: N x M, B: M x N).
    /// </summary>
    public int? M { get; init; }

    /// <summary>
    /// Число раундов K (например, для вероятностного теста Миллера–Рабина).
    /// </summary>
    public int? K { get; init; }

    /// <summary>
    /// Основание X для алгоритмов возведения в степень.
    /// </summary>
    public double? X { get; init; }

    /// <summary>
    /// Принудительный пересчёт без использования сохранённого кэша.
    /// </summary>
    public bool ForceRecalculate { get; init; }

    /// <summary>
    /// Вычисляет детерминированный SHA-256 хэш всех параметров, влияющих на замеры.
    /// Не включает ForceRecalculate и случайные данные входа.
    /// </summary>
    public string ComputeConfigHash()
    {
        string raw = $"NMax={NMax};NStep={NStep};Runs={RunsPerN};M={M};K={K};X={(X.HasValue ? X.Value.ToString("R", System.Globalization.CultureInfo.InvariantCulture) : "null")}";
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    /// <summary>
    /// Сериализует конфигурацию в JSON-строку.
    /// </summary>
    public string ToJson() => JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = false });

    /// <summary>
    /// Десериализует конфигурацию из JSON-строки.
    /// </summary>
    public static ExperimentConfig FromJson(string json) =>
        JsonSerializer.Deserialize<ExperimentConfig>(json) ?? new ExperimentConfig();
}
