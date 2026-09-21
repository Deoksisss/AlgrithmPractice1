using AlgorithmPractice1.Core.Abstractions;
using AlgorithmPractice1.Core.Models;

namespace AlgorithmPractice1.Core.Algorithms.Part1_Vectors;

/// <summary>
/// 3. Произведение элементов вектора (сложность O(n)).
/// </summary>
public sealed class VectorProductAlgorithm : AlgorithmBase<double[], double>
{
    public override string Id => "VectorProduct";
    public override string DisplayName => "Произведение элементов вектора";
    public override string Description => "Последовательное перемножение всех элементов вектора. Теоретическая сложность O(n).";
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
        // Генерируем числа близкие к 1.0, чтобы избежать переполнения/исчезновения разрядов
        var random = new Random(42 + n);
        var array = new double[n];
        for (int i = 0; i < n; i++)
        {
            array[i] = 1.0 + (random.NextDouble() - 0.5) * 1e-4;
        }
        return array;
    }

    public override double ExecuteTyped(double[] input, MeasurementContext? context)
    {
        double product = 1.0;
        for (int i = 0; i < input.Length; i++)
        {
            product *= input[i];
        }
        return product;
    }
}
