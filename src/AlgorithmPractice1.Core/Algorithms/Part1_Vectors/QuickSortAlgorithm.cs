using AlgorithmPractice1.Core.Abstractions;
using AlgorithmPractice1.Core.Models;

namespace AlgorithmPractice1.Core.Algorithms.Part1_Vectors;

/// <summary>
/// 6. Быстрая сортировка (Quick sort, сложность O(n log n)).
/// </summary>
public sealed class QuickSortAlgorithm : AlgorithmBase<int[], int[]>
{
    public override string Id => "QuickSort";
    public override string DisplayName => "Быстрая сортировка (Quick sort)";
    public override string Description => "Сортировка разделением по опорному элементу (O(n log n)).";
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
        if (arr.Length > 1)
        {
            Sort(arr, 0, arr.Length - 1);
        }
        return arr;
    }

    private static void Sort(int[] arr, int left, int right)
    {
        if (left >= right) return;

        int pivotIndex = Partition(arr, left, right);
        Sort(arr, left, pivotIndex);
        Sort(arr, pivotIndex + 1, right);
    }

    private static int Partition(int[] arr, int left, int right)
    {
        int pivot = arr[left + (right - left) / 2];
        int i = left - 1;
        int j = right + 1;

        while (true)
        {
            do { i++; } while (arr[i] < pivot);
            do { j--; } while (arr[j] > pivot);

            if (i >= j) return j;

            (arr[i], arr[j]) = (arr[j], arr[i]);
        }
    }
}
