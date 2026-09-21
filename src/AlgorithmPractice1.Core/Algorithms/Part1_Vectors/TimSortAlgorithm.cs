using AlgorithmPractice1.Core.Abstractions;
using AlgorithmPractice1.Core.Models;

namespace AlgorithmPractice1.Core.Algorithms.Part1_Vectors;

/// <summary>
/// 7. Timsort / стандартная сортировка .NET (Array.Sort, сложность O(n log n)).
/// </summary>
public sealed class TimSortAlgorithm : AlgorithmBase<int[], int[]>
{
    public override string Id => "TimSort";
    public override string DisplayName => "Timsort (.NET Array.Sort)";
    public override string Description => "Стандартная гибридная сортировка .NET (аналог Timsort / Introsort, O(n log n)).";
    public override AlgorithmCategory Category => AlgorithmCategory.Vectors;
    public override MeasurementType MeasurementType => MeasurementType.Time;
    public override ComplexityFunctionType TheoreticalComplexity => ComplexityFunctionType.Linearithmic;

    public override ExperimentConfig DefaultConfig => new()
    {
        NMax = 60000,
        NStep = 5000,
        RunsPerN = 5
    };

    public override int[] GenerateTypedInput(int n, ExperimentConfig config)
    {
        var random = new Random(42 + n);
        var array = new int[n];
        for (int i = 0; i < n; i++)
        {
            array[i] = random.Next();
        }
        return array;
    }

    public override int[] ExecuteTyped(int[] input, MeasurementContext? context)
    {
        var arr = (int[])input.Clone();
        Array.Sort(arr);
        return arr;
    }
}
