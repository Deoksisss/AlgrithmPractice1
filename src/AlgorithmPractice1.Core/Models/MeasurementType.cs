namespace AlgorithmPractice1.Core.Models;

/// <summary>
/// Тип измеряемой величины: время выполнения или количество элементарных шагов.
/// </summary>
public enum MeasurementType
{
    /// <summary>
    /// Измерение времени (Stopwatch тики / миллисекунды)
    /// </summary>
    Time,

    /// <summary>
    /// Подсчёт точного количества операций/шагов
    /// </summary>
    Steps
}
