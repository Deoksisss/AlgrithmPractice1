using AlgorithmPractice1.Core.Abstractions;
using AlgorithmPractice1.Core.Models;

namespace AlgorithmPractice1.Core.Algorithms.Part1_Vectors;

/// <summary>
/// 5. Сортировка пузырьком (Bubble sort, сложность O(n^2)).
/// </summary>
public sealed class BubbleSortAlgorithm : AlgorithmBase<int[], int[]>
{
    public override string Id => "BubbleSort";
    public override string DisplayName => "Сортировка пузырьком";
    public override string Description => "Классическая квадратичная сортировка обменами соседних элементов (O(n²)).";
    public override AlgorithmCategory Category => AlgorithmCategory.Vectors;
    public override MeasurementType MeasurementType => MeasurementType.Time;
    public override ComplexityFunctionType TheoreticalComplexity => ComplexityFunctionType.Quadratic;

    public override ExperimentConfig DefaultConfig => new()
    {
        NMax = 3000,
        NStep = 200,
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
        int n = input.Length;
        for (int i = 0; i < n - 1; i++)
        {
            for (int j = 0; j < n - 1 - i; j++)
            {
                if (input[j] > input[j + 1])
                {
                    int temp = input[j];
                    input[j] = input[j + 1];
                    input[j + 1] = temp;
                }
            }
        }

        return input;
    }
}
