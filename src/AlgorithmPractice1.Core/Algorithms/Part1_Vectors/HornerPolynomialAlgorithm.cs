using AlgorithmPractice1.Core.Abstractions;
using AlgorithmPractice1.Core.Models;

namespace AlgorithmPractice1.Core.Algorithms.Part1_Vectors;

/// <summary>
/// 4b. Вычисление многочлена методом Горнера в точке x = 1.5 (сложность O(n)).
/// </summary>
public sealed class HornerPolynomialAlgorithm : AlgorithmBase<double[], double>
{
    public const double DefaultX = 1.5;

    public override string Id => "HornerPolynomial";
    public override string DisplayName => "Многочлен (метод Горнера)";
    public override string Description => "Вычисление значения полинома в точке x = 1.5 по схеме Горнера (O(n)).";
    public override AlgorithmCategory Category => AlgorithmCategory.Vectors;
    public override MeasurementType MeasurementType => MeasurementType.Time;
    public override ComplexityFunctionType TheoreticalComplexity => ComplexityFunctionType.Linear;

    public override ExperimentConfig DefaultConfig => new()
    {
        NMax = 100000,
        NStep = 10000,
        RunsPerN = 5,
        X = DefaultX
    };

    public override double[] GenerateTypedInput(int n, ExperimentConfig config)
    {
        var random = new Random(42 + n);
        var array = new double[n];
        for (int i = 0; i < n; i++)
        {
            array[i] = (random.NextDouble() - 0.5) / 100.0;
        }
        return array;
    }

    public override double ExecuteTyped(double[] input, MeasurementContext? context)
    {
        if (input.Length == 0) return 0.0;

        double x = DefaultX;
        double result = input[^1];

        for (int i = input.Length - 2; i >= 0; i--)
        {
            result = result * x + input[i];
        }

        return result;
    }
}
