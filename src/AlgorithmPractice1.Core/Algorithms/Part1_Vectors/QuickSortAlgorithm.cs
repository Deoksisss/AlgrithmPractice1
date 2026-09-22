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
        if (input.Length > 1)
        {
            Sort(input, 0, input.Length - 1);
        }
        return input;
    }

    private static void Sort(int[] a, int left, int right)
    {
        if (left >= right) return;

        int pivot = a[left + (right - left) / 2];
        int i = left, j = right;

        while (i <= j)
        {
            while (a[i] < pivot) i++;
            while (a[j] > pivot) j--;
            if (i <= j)
            {
                int temp = a[i];
                a[i] = a[j];
                a[j] = temp;
                i++;
                j--;
            }
        }

        Sort(a, left, j);
        Sort(a, i, right);
    }
}
