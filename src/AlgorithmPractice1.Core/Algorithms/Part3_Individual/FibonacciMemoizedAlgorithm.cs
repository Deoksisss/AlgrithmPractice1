using AlgorithmPractice1.Core.Abstractions;
using AlgorithmPractice1.Core.CallTree;
using AlgorithmPractice1.Core.Models;
using System.Collections.Generic;

namespace AlgorithmPractice1.Core.Algorithms.Part3_Individual;

/// <summary>
/// Рекурсия с мемоизацией (top-down DP). Сложность O(n) по времени, O(n) по памяти.
/// </summary>
public sealed class FibonacciMemoizedAlgorithm : AlgorithmBase<int, long>
{
    public override string Id => "FibonacciMemoized";
    public override string DisplayName => "Фибоначчи (рекурсия + мемоизация)";
    public override string Description =>
        "Рекурсивная реализация с кэшированием результатов подзадач (мемоизация). " +
        "Каждое F(k) вычисляется ровно один раз.";
    public override AlgorithmCategory Category => AlgorithmCategory.Individual;
    public override MeasurementType MeasurementType => MeasurementType.Time;
    public override ComplexityFunctionType TheoreticalComplexity => ComplexityFunctionType.Linear;
    public override bool IsRecursive => true;

    public override ExperimentConfig DefaultConfig => new()
    {
        NMax = 90,     // Ограничение long: F(93) переполняет long
        NStep = 1,
        RunsPerN = 5
    };

    public override int GenerateTypedInput(int n, ExperimentConfig config) => n;

    public override long ExecuteTyped(int input, MeasurementContext? context)
    {
        // Кэш создаётся заново на каждый вызов — иначе замеры исказятся
        var cache = new Dictionary<int, long>();
        return Fib(input, cache, context);
    }

    private long Fib(int n, Dictionary<int, long> cache, MeasurementContext? ctx)
    {
        // Проверка кэша: если уже считали, возвращаем мгновенно
        if (cache.TryGetValue(n, out var cached))
            return cached;

        // Увеличиваем шаг только при реальном вычислении нового значения
        ctx?.IncrementSteps();

        long result;
        if (n <= 1)
        {
            result = n;
        }
        else if (CallTreeTracker.IsTracking)
        {
            result = OutputCallTree(() => Fib(n - 1, cache, ctx)) + OutputCallTree(() => Fib(n - 2, cache, ctx));
        }
        else
        {
            result = Fib(n - 1, cache, ctx) + Fib(n - 2, cache, ctx);
        }
        
        // Сохраняем в кэш
        cache[n] = result;
        return result;
    }
}
