namespace AlgorithmPractice1.Core.Models;

/// <summary>
/// Сырая точка замера для одного запуска алгоритма.
/// </summary>
public record MeasurementPoint
{
    public long Id { get; init; }
    public long SessionAlgorithmId { get; init; }
    public int N { get; init; }
    public int? M { get; init; }
    public int RunIndex { get; init; }
    public long? ElapsedTicks { get; init; }
    public long? StepCount { get; init; }

    /// <summary>
    /// Время выполнения в миллисекундах (вычисляется из тиков Stopwatch).
    /// </summary>
    public double? ElapsedMilliseconds =>
        ElapsedTicks.HasValue ? (double)ElapsedTicks.Value / System.Diagnostics.Stopwatch.Frequency * 1000.0 : null;

    /// <summary>
    /// Время выполнения в секундах.
    /// </summary>
    public double? ElapsedSeconds =>
        ElapsedTicks.HasValue ? (double)ElapsedTicks.Value / System.Diagnostics.Stopwatch.Frequency : null;
}
