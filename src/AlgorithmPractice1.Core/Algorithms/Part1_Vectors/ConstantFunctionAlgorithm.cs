using AlgorithmPractice1.Core.Abstractions;
using AlgorithmPractice1.Core.Models;

namespace AlgorithmPractice1.Core.Algorithms.Part1_Vectors;

/// <summary>
/// 1. Постоянная функция: f(v) = 1 (сложность O(1)).
/// </summary>
public sealed class ConstantFunctionAlgorithm : AlgorithmBase<double[], double>
{
    public override string Id => "ConstantFunction";
    public override string DisplayName => "Постоянная функция f(v)=1";
    public override string Description => "Возвращает 1.0 независимо от размера вектора v. Теоретическая сложность O(1).";
    public override AlgorithmCategory Category => AlgorithmCategory.Vectors;
    public override MeasurementType MeasurementType => MeasurementType.Time;
    public override ComplexityFunctionType TheoreticalComplexity => ComplexityFunctionType.Constant;

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
        return 1.0;
    }
}
