using AlgorithmPractice1.Core.Abstractions;
using AlgorithmPractice1.Core.Models;

namespace AlgorithmPractice1.Core.Algorithms.Part3_Individual;

/// <summary>
/// Нахождение всех подмножеств множеств индуктивно (сложность O(2^n)).
/// </summary>
public sealed class InductiveSubsetsAlgorithm : AlgorithmBase<int[], List<List<int>>>
{
    public override string Id => "InductiveSubsets";
    public override string DisplayName => "Нахождение всех подмножеств множеств индуктивно";
    public override string Description =>
        "Индуктивное нахождение всех подмножеств множества: начиная с пустого множества, " +
        "на каждом шаге копируются уже найденные подмножества с добавлением нового элемента в конец копий (O(2^n)).";
    public override AlgorithmCategory Category => AlgorithmCategory.Individual;
    public override MeasurementType MeasurementType => MeasurementType.Time;
    public override ComplexityFunctionType TheoreticalComplexity => ComplexityFunctionType.Exponential;

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
        var subsets = new List<List<int>> { new List<int>() };
        context?.IncrementSteps();

        foreach (int element in input)
        {
            int count = subsets.Count;
            for (int i = 0; i < count; i++)
            {
                var newSubset = new List<int>(subsets[i]) { element };
                subsets.Add(newSubset);
                context?.IncrementSteps();
            }
        }

        return subsets;
    }
}
