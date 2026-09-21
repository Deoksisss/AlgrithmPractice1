using AlgorithmPractice1.Core.Abstractions;
using AlgorithmPractice1.Core.Models;

namespace AlgorithmPractice1.Core.Algorithms.Part1_Vectors;

/// <summary>
/// 2. Сумма элементов вектора (сложность O(n)).
/// </summary>
public sealed class VectorSumAlgorithm : AlgorithmBase<double[], double>
{
    public override string Id => "VectorSum";
    public override string DisplayName => "Сумма элементов вектора";
    public override string Description => "Последовательное суммирование всех элементов вектора. Теоретическая сложность O(n).";
    public override AlgorithmCategory Category => AlgorithmCategory.Vectors;
    public override MeasurementType MeasurementType => MeasurementType.Time;
    public override ComplexityFunctionType TheoreticalComplexity => ComplexityFunctionType.Linear;

    public override ExperimentConfig DefaultConfig => new()
    {
        NMax = 100000,
        NStep = 10000,
        RunsPerN = 5
    };

    public override double[] GenerateTypedInput(int n, ExperimentConfig config)
    {
        var random = new Random(42 + n);
        var array = new double[n];
        for (int i = 0; i < n; i++)
        {
            array[i] = random.NextDouble();
        }
        return array;
    }

    public override double ExecuteTyped(double[] input, MeasurementContext? context)
    {
        double sum = 0.0;
        for (int i = 0; i < input.Length; i++)
        {
            sum += input[i];
        }
        return sum;
    }
}
