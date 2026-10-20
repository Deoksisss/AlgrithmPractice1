using System.Numerics;
using AlgorithmPractice1.Core.Abstractions;
using AlgorithmPractice1.Core.Models;

namespace AlgorithmPractice1.Core.Algorithms.Part3_Individual;


public sealed class FibonacciIterativeAlgorithm : AlgorithmBase<int, long>
{
    public override string Id => "FibonacciIterative";
    public override string DisplayName => "Алгоритм Фибоначчи (итеративный)";
    public override string Description => "Вычисляет n-е число Фибоначчи итеративно за O(n).";
    public override AlgorithmCategory Category => AlgorithmCategory.Individual;
    public override MeasurementType MeasurementType => MeasurementType.Time;
    public override ComplexityFunctionType TheoreticalComplexity => ComplexityFunctionType.Linear;

    public override ExperimentConfig DefaultConfig => new()
    {
        NMax = 90, // Можно гонять хоть до 90, дальше long переполнится (вернет отрицательное число)
        NStep = 1,
        RunsPerN = 5
    };

    public override int GenerateTypedInput(int n, ExperimentConfig config) => n;

    public override long ExecuteTyped(int input, MeasurementContext? context)
    {
        if (input <= 0) return 0;
        if (input == 1) return 1;

        long a = 0;
        long b = 1;

        for (int i = 2; i <= input; i++)
        {
            context?.IncrementSteps();
            long temp = a + b;
            a = b;
            b = temp;
        }

        return b;
    }
}