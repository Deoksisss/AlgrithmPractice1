using AlgorithmPractice1.Core.Abstractions;
using AlgorithmPractice1.Core.CallTree;
using AlgorithmPractice1.Core.Models;

namespace AlgorithmPractice1.Core.Algorithms.Part3_Individual;

/// <summary>
/// Нахождение всех подмножеств множеств индуктивно (рекурсивный вызов n-1, сложность O(2^n)).
/// </summary>
public sealed class InductiveSubsetsAlgorithm : AlgorithmBase<int[], List<List<int>>>
{
    public override string Id => "InductiveSubsets";
    public override string DisplayName => "Нахождение всех подмножеств множеств индуктивно";
    public override string Description =>
        "Индуктивное нахождение всех подмножеств множества: рекурсивно находится решение для (n-1), " +
        "после чего подмножества копируются с добавлением n-го элемента в конец копий (O(2^n)).";
    public override AlgorithmCategory Category => AlgorithmCategory.Individual;
    public override MeasurementType MeasurementType => MeasurementType.Time;
    public override ComplexityFunctionType TheoreticalComplexity => ComplexityFunctionType.Exponential;
    public override bool IsRecursive => true;

    public override ExperimentConfig DefaultConfig => new()
    {
        NMax = 16,
        NStep = 1,
        RunsPerN = 5
    };

    public override int[] GenerateTypedInput(int n, ExperimentConfig config)
    {
        var array = new int[n];
        for (int i = 0; i < n; i++)
        {
            array[i] = i;
        }
        return array;
    }

    public override List<List<int>> ExecuteTyped(int[] input, MeasurementContext? context)
    {
        return FindSubsets(input, input.Length, context);
    }

    private List<List<int>> FindSubsets(int[] input, int n, MeasurementContext? context)
    {
        if (n <= 0)
        {
            context?.IncrementSteps();
            return new List<List<int>> { new List<int>() };
        }

        List<List<int>> previous;
        if (CallTreeTracker.IsTracking)
        {
            previous = OutputCallTree(() => FindSubsets(input, n - 1, context));
        }
        else
        {
            previous = FindSubsets(input, n - 1, context);
        }

        int currentElement = input[n - 1];
        var result = new List<List<int>>(previous);

        int count = previous.Count;
        for (int i = 0; i < count; i++)
        {
            var newSubset = new List<int>(previous[i]) { currentElement };
            result.Add(newSubset);
            context?.IncrementSteps();
        }

        return result;
    }
}
