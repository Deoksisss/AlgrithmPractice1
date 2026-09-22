using AlgorithmPractice1.Core.Abstractions;
using AlgorithmPractice1.Core.Models;

namespace AlgorithmPractice1.Core.Algorithms.Part4_Exponentiation;

public record ExponentiationInput(double BaseX, int ExponentN);

/// <summary>
/// 12. Простой итеративный алгоритм возведения в степень (n-1 умножений).
/// Измеряется время выполнения (мс). Сложность O(n).
/// </summary>
public sealed class IterativeExponentiationAlgorithm : AlgorithmBase<ExponentiationInput, double>
{
    public const double DefaultBase = 1.0001;

    public override string Id => "IterativeExponentiation";
    public override string DisplayName => "Возведение в степень (итеративное)";
    public override string Description => "Последовательное перемножение x сама на себя n-1 раз. Измеряется время выполнения (O(n)).";
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
        double x = input.BaseX;
        int n = input.ExponentN;

        if (n <= 0) return 1.0;
        if (n == 1) return x;

        int iterations = context != null ? 1 : 2000;
        double result = 1.0;
        for (int iter = 0; iter < iterations; iter++)
        {
            result = x;
            for (int i = 1; i < n; i++)
            {
                result *= x;
                context?.IncrementSteps();
            }
        }

        return result;
    }
}
