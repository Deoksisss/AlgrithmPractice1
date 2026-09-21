namespace AlgorithmPractice1.Core.Models;

/// <summary>
/// Контекст для алгоритмов, ведущих явный подсчёт шагов/операций (Часть IV).
/// </summary>
public sealed class MeasurementContext
{
    private long _stepCount;

    public long StepCount => _stepCount;

    public void IncrementSteps(long delta = 1)
    {
        _stepCount += delta;
    }

    public void Reset()
    {
        _stepCount = 0;
    }
}
