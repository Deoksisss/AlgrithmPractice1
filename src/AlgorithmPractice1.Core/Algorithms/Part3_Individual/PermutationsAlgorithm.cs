using AlgorithmPractice1.Core.Abstractions;
using AlgorithmPractice1.Core.CallTree;
using AlgorithmPractice1.Core.Models;

namespace AlgorithmPractice1.Core.Algorithms.Part3_Individual;

/// <summary>
/// Генерация всех перестановок (Permutations, сложность O(n!))
/// </summary>
public sealed class PermutationsAlgorithm : AlgorithmBase<int[], long>
{
    public override string Id => "Permutations";
    public override string DisplayName => "Алгоритм генерации всех перестановок";
    public override string Description => "Дано множество из n элементов (например, {1, 2, 3}). Генерирует все возможные перестановки этих элементов.";
    public override AlgorithmCategory Category => AlgorithmCategory.Individual;
    public override MeasurementType MeasurementType => MeasurementType.Time;
    public override ComplexityFunctionType TheoreticalComplexity => ComplexityFunctionType.Faсtorial;
    public override bool IsRecursive => true;
    
    public override ExperimentConfig DefaultConfig => new()
    {
        NMax = 10,
        NStep = 1,
        RunsPerN = 5
    };

    public override int[] GenerateTypedInput(int n, ExperimentConfig config)
    {
        var array = new int[n];
        for (int i = 0; i < n; i++)
        {
            array[i] = i + 1;
        }
        return array;
    }
    
    public override long ExecuteTyped(int[] input, MeasurementContext? context)
    {
        if (input.Length == 0) return 0;
        if (input.Length == 1)
        {
            context?.IncrementSteps();
            return 1;
        }

        return Permute(input, 0, context);
    }

    private long Permute(int[] input, int start, MeasurementContext? context)
    {
        if (start == input.Length - 1)
        {
            context?.IncrementSteps();
            return 1;
        }
        
        long count = 0;
        for (int i = start; i < input.Length; i++)
        {
            Swap(input, start, i);

            if (CallTreeTracker.IsTracking)
            {
                count += OutputCallTree(() => Permute(input, start + 1, context));
            }
            else
            {
                count += Permute(input, start + 1, context);
            }

            Swap(input, start, i);
        }

        return count;
    }
    
    private static void Swap<T>(T[] a, int i, int j)
    {
        if (i == j) return;
        (a[i], a[j]) = (a[j], a[i]);
    }
}
