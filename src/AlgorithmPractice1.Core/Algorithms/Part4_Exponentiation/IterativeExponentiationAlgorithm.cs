using AlgorithmPractice1.Core.Abstractions;
using AlgorithmPractice1.Core.Models;

namespace AlgorithmPractice1.Core.Algorithms.Part4_Exponentiation;

public record ExponentiationInput(double BaseX, int ExponentN);

/// <summary>
/// 12. Простой итеративный алгоритм возведения в степень (n-1 умножений).
/// Измеряется количество операций умножения. Сложность O(n).
/// </summary>
public sealed class IterativeExponentiationAlgorithm : AlgorithmBase<ExponentiationInput, double>
{
    public const double DefaultBase = 1.0001;

    public override string Id => "IterativeExponentiation";
    public override string DisplayName => "Возведение в степень (итеративное)";
    public override string Description => "Последовательное перемножение x сама на себя n-1 раз. Измеряется количество умножений (O(n)).";
    public override AlgorithmCategory Category => AlgorithmCategory.Exponentiation;
    public override MeasurementType MeasurementType => MeasurementType.Steps;
    public override ComplexityFunctionType TheoreticalComplexity => ComplexityFunctionType.Linear;

    public override ExperimentConfig DefaultConfig => new()
    {
        NMax = 1000,
        NStep = 50,
        RunsPerN = 1,
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

        double result = x;
        for (int i = 1; i < n; i++)
        {
            result *= x;
            context?.IncrementSteps();
        }

        return result;
    }
}
