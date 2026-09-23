using AlgorithmPractice1.Core.Abstractions;
using AlgorithmPractice1.Core.Models;

namespace AlgorithmPractice1.Core.Algorithms.Part4_Exponentiation;

/// <summary>
/// 16. Классический алгоритм возведения в степень (быстрое итеративное возведение, O(log n)).
/// Сложность по памяти O(1), так как нет рекурсии (цикл while, отсутствие риска переполнения стека).
/// Используются быстрые битовые операции n & 1 и n >>= 1.
/// Измеряется время выполнения (мс).
/// </summary>
public sealed class ClassicalExponentiationAlgorithm : AlgorithmBase<ExponentiationInput, double>
{
    public const double DefaultBase = 1.0001;

    public override string Id => "ClassicalExponentiation";
    public override string DisplayName => "Классический алгоритм возведения в степень";
    public override string Description => "Классический итеративный алгоритм быстрого возведения в степень за O(log n) без рекурсии (память O(1), битовые операции n & 1 и n >>= 1).";
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
            result = PowerClassical(input.BaseX, input.ExponentN, context);
        }
        return result;
    }

    public static double PowerClassical(double x, long n, MeasurementContext? context)
    {
        if (n <= 0) return 1.0;

        double result = 1.0;
        double currentBase = x;

        while (n > 0)
        {
            if ((n & 1) == 1)
            {
                result *= currentBase;
                context?.IncrementSteps();
            }

            n >>= 1;
            if (n > 0)
            {
                currentBase *= currentBase;
                context?.IncrementSteps();
            }
        }

        return result;
    }
}
