using AlgorithmPractice1.Core.Abstractions;
using AlgorithmPractice1.Core.CallTree;
using AlgorithmPractice1.Core.Models;

namespace AlgorithmPractice1.Core.Algorithms.Part3_Individual;

/// <summary>
/// Классическая головоломка «Ханойские башни» (сложность O(2^n)).
/// N задаёт количество колец/дисков. Число перемещений равно 2^n - 1.
/// </summary>
public sealed class HanoiTowersAlgorithm : AlgorithmBase<int, long>
{
    public override string Id => "HanoiTowers";
    public override string DisplayName => "Ханойские башни";
    public override string Description =>
        "Классическая задача о Ханойских башнях. Перекладывание n дисков со стержня A на стержень C " +
        "с использованием промежуточного стержня B по правилам головоломки (O(2^n)).";
    public override AlgorithmCategory Category => AlgorithmCategory.Individual;
    public override MeasurementType MeasurementType => MeasurementType.Time;
    public override ComplexityFunctionType TheoreticalComplexity => ComplexityFunctionType.Exponential;
    public override bool IsRecursive => true;

    public override ExperimentConfig DefaultConfig => new()
    {
        NMax = 20,
        NStep = 1,
        RunsPerN = 3
    };

    public override int GenerateTypedInput(int n, ExperimentConfig config) => n;

    public override long ExecuteTyped(int input, MeasurementContext? context)
    {
        if (input <= 0) return 0;
        return SolveHanoi(input, "A", "C", "B", context);
    }

    private long SolveHanoi(int n, string fromRod, string toRod, string auxRod, MeasurementContext? context)
    {
        if (n <= 0) return 0;

        if (n == 1)
        {
            context?.IncrementSteps();
            return 1;
        }

        long moves = 0;
        if (CallTreeTracker.IsTracking)
        {
            moves += OutputCallTree(() => SolveHanoi(n - 1, fromRod, auxRod, toRod, context));
            context?.IncrementSteps();
            moves += 1;
            moves += OutputCallTree(() => SolveHanoi(n - 1, auxRod, toRod, fromRod, context));
        }
        else
        {
            moves += SolveHanoi(n - 1, fromRod, auxRod, toRod, context);
            context?.IncrementSteps();
            moves += 1;
            moves += SolveHanoi(n - 1, auxRod, toRod, fromRod, context);
        }

        return moves;
    }
}
