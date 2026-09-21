namespace AlgorithmPractice1.Core.Models;

/// <summary>
/// Сущность сессии эксперимента в БД (пакетный запуск нескольких алгоритмов).
/// </summary>
public record BenchmarkSession
{
    public long Id { get; init; }

    /// <summary>
    /// Дата и время проведения сессии в формате ISO 8601.
    /// </summary>
    public string CreatedAt { get; init; } = DateTime.UtcNow.ToString("o");

    /// <summary>
    /// Пользовательская метка или автосгенерированное название сессии.
    /// </summary>
    public string? Label { get; init; }

    /// <summary>
    /// Алгоритмы, вошедшие в данную сессию.
    /// </summary>
    public List<SessionAlgorithm> Algorithms { get; init; } = new();
}

/// <summary>
/// Запуск одного алгоритма внутри сессии.
/// </summary>
public record SessionAlgorithm
{
    public long Id { get; init; }
    public long SessionId { get; init; }
    public string AlgorithmId { get; init; } = string.Empty;
    public string ConfigJson { get; init; } = string.Empty;
    public string ConfigHash { get; init; } = string.Empty;

    public ExperimentConfig Config => ExperimentConfig.FromJson(ConfigJson);

    public List<MeasurementPoint> Measurements { get; init; } = new();
    public ApproximationResult? Approximation { get; set; }
}
