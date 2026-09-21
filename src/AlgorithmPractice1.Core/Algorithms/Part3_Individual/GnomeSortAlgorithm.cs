using AlgorithmPractice1.Core.Abstractions;
using AlgorithmPractice1.Core.Models;

namespace AlgorithmPractice1.Core.Algorithms.Part3_Individual;

/// <summary>
/// 9. Гномья сортировка (Gnome sort, сложность O(n^2)).
/// </summary>
public sealed class GnomeSortAlgorithm : AlgorithmBase<int[], int[]>
{
    public override string Id => "GnomeSort";
    public override string DisplayName => "Гномья сортировка (Gnome sort)";
    public override string Description => "Сортировка перемещением элемента на нужную позицию назад при нарушении порядка (O(n²)).";
    public override AlgorithmCategory Category => AlgorithmCategory.Individual;
    public override MeasurementType MeasurementType => MeasurementType.Time;
    public override ComplexityFunctionType TheoreticalComplexity => ComplexityFunctionType.Quadratic;

    public override ExperimentConfig DefaultConfig => new()
    {
        NMax = 2500,
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
        var arr = (int[])input.Clone();
        int n = arr.Length;
        int index = 0;

        while (index < n)
        {
            if (index == 0 || arr[index] >= arr[index - 1])
            {
                index++;
            }
            else
            {
                (arr[index], arr[index - 1]) = (arr[index - 1], arr[index]);
                index--;
            }
        }

        return arr;
    }
}
