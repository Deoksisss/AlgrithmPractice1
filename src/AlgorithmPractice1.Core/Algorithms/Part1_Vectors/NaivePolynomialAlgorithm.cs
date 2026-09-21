using AlgorithmPractice1.Core.Abstractions;
using AlgorithmPractice1.Core.Models;

namespace AlgorithmPractice1.Core.Algorithms.Part1_Vectors;

/// <summary>
/// 4a. Вычисление многочлена наивным методом: P(x) = sum(a_k * x^k), где x = 1.5 (сложность O(n^2)).
/// Для каждого k степень x^k считается отдельным циклом умножений.
/// </summary>
public sealed class NaivePolynomialAlgorithm : AlgorithmBase<double[], double>
{
    public const double DefaultX = 1.5;

    public override string Id => "NaivePolynomial";
    public override string DisplayName => "Многочлен (наивный метод)";
    public override string Description => "Вычисление значения полинома в точке x = 1.5 с повторным вычислением степеней x^k (O(n²)).";
    public override AlgorithmCategory Category => AlgorithmCategory.Vectors;
    public override MeasurementType MeasurementType => MeasurementType.Time;
    public override ComplexityFunctionType TheoreticalComplexity => ComplexityFunctionType.Quadratic;

    public override ExperimentConfig DefaultConfig => new()
    {
        NMax = 4000,
        NStep = 300,
        RunsPerN = 5,
        X = DefaultX
    };

    public override double[] GenerateTypedInput(int n, ExperimentConfig config)
    {
        var random = new Random(42 + n);
        var array = new double[n];
        for (int i = 0; i < n; i++)
        {
            // Небольшие коэффициенты, чтобы предотвратить переполнение при больших степенях
            array[i] = (random.NextDouble() - 0.5) / 100.0;
        }
        return array;
    }

    public override double ExecuteTyped(double[] input, MeasurementContext? context)
    {
        double x = DefaultX;
        double sum = 0.0;

        for (int k = 0; k < input.Length; k++)
        {
            double xk = 1.0;
            for (int j = 0; j < k; j++)
            {
                xk *= x;
            }
            sum += input[k] * xk;
        }

        return sum;
    }
}
