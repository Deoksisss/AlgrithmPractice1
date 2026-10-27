using AlgorithmPractice1.Core.Abstractions;
using AlgorithmPractice1.Core.CallTree;
using AlgorithmPractice1.Core.Models;

namespace AlgorithmPractice1.Core.Algorithms.Part3_Individual;

/// <summary>
/// Наивная рекурсия: F(n) = F(n-1) + F(n-2). Сложность O(φ^n) — экспоненциальная.
/// </summary>
public sealed class FibonacciNaiveRecursiveAlgorithm : AlgorithmBase<int, long>
{
    public override string Id => "FibonacciNaiveRecursive";
    public override string DisplayName => "Фибоначчи (наивная рекурсия)";
    public override string Description =>
        "Наивная рекурсивная реализация F(n)=F(n-1)+F(n-2). " +
        "Экспоненциальная сложность из-за повторных вычислений подзадач.";
    public override AlgorithmCategory Category => AlgorithmCategory.Individual;
    public override MeasurementType MeasurementType => MeasurementType.Time;
    public override ComplexityFunctionType TheoreticalComplexity => ComplexityFunctionType.Exponential;
    public override bool IsRecursive => true;

    public override ExperimentConfig DefaultConfig => new()
    {
        NMax = 32,     // Больше — слишком долго для наивной рекурсии
        NStep = 1,
        RunsPerN = 3   // Не 5, чтобы лабораторная не шла вечность
    };

    public override int GenerateTypedInput(int n, ExperimentConfig config) => n;

    public override long ExecuteTyped(int input, MeasurementContext? context)
    {
        return Fib(input, context);
    }

    private long Fib(int n, MeasurementContext? ctx)
    {
        ctx?.IncrementSteps();
        if (n <= 1) return n;

        if (CallTreeTracker.IsTracking)
        {
            return OutputCallTree(() => Fib(n - 1, ctx)) + OutputCallTree(() => Fib(n - 2, ctx));
        }

        return Fib(n - 1, ctx) + Fib(n - 2, ctx);
    }
}
