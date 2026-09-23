using AlgorithmPractice1.Core.Abstractions;
using AlgorithmPractice1.Core.Models;

namespace AlgorithmPractice1.Core.Algorithms.Part4_Exponentiation;

/// <summary>
/// 14. Быстрое бинарное рекурсивное возведение в степень (сложность O(log n)).
/// Измеряется время выполнения (мс).
/// </summary>
public sealed class BinaryExponentiationAlgorithm : AlgorithmBase<ExponentiationInput, double>
{
    public const double DefaultBase = 1.0001;

    public override string Id => "BinaryExponentiation";
    public override string DisplayName => "Возведение в степень (бинарное)";
    public override string Description => "Быстрое бинарное рекурсивное возведение в степень за логарифмическое время (O(log n)), память O(log n).";
    public override AlgorithmCategory Category => AlgorithmCategory.Exponentiation;
    public override MeasurementType MeasurementType => MeasurementType.Time;
    public override ComplexityFunctionType TheoreticalComplexity => ComplexityFunctionType.Logarithmic;

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
            result = PowerBinary(input.BaseX, input.ExponentN, context);
        }
        return result;
    }

    public static double PowerBinary(double x, long n, MeasurementContext? context)
    {
        if (n <= 0) return 1.0;
        if (n == 1) return x;

        if ((n & 1) == 1)
        {
            context?.IncrementSteps();
            return x * PowerBinary(x, n - 1, context);
        }
        else
        {
            context?.IncrementSteps();
            double half = PowerBinary(x, n >> 1, context);
            return half * half;
        }
    }
}
