using AlgorithmPractice1.Core.Abstractions;
using AlgorithmPractice1.Core.Models;

namespace AlgorithmPractice1.Core.Algorithms.Part4_Exponentiation;

/// <summary>
/// 13. Рекурсивный алгоритм возведения в степень: x^n = x * x^(n-1).
/// Измеряется время выполнения (мс). Сложность O(n).
/// </summary>
public sealed class RecursiveExponentiationAlgorithm : AlgorithmBase<ExponentiationInput, double>
{
    public const double DefaultBase = 1.0001;

    public override string Id => "RecursiveExponentiation";
    public override string DisplayName => "Возведение в степень (рекурсивное)";
    public override string Description => "Рекурсивное определение x^n = x · x^(n-1). Измеряется время выполнения (O(n)).";
    public override AlgorithmCategory Category => AlgorithmCategory.Exponentiation;
    public override MeasurementType MeasurementType => MeasurementType.Time;
    public override ComplexityFunctionType TheoreticalComplexity => ComplexityFunctionType.Linear;

    public override ExperimentConfig DefaultConfig => new()
    {
        NMax = 150,
        NStep = 25,
        RunsPerN = 3,
        X = DefaultBase
    };

    public override ExponentiationInput GenerateTypedInput(int n, ExperimentConfig config)
    {
        double x = config.X.GetValueOrDefault(DefaultBase);
        return new ExponentiationInput(x, n);
    }

    public override double ExecuteTyped(ExponentiationInput input, MeasurementContext? context)
    {
        int iterations = context != null ? 1 : 2000;
        double result = 1.0;
        for (int iter = 0; iter < iterations; iter++)
        {
            result = PowerRecursive(input.BaseX, input.ExponentN, context);
        }
        return result;
    }

    private static double PowerRecursive(double x, int n, MeasurementContext? context)
    {
        if (n <= 0) return 1.0;
        if (n == 1) return x;

        context?.IncrementSteps();
        return x * PowerRecursive(x, n - 1, context);
    }
}
