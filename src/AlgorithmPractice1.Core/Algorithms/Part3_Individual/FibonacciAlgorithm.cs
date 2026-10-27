using System.Numerics;
using AlgorithmPractice1.Core.Abstractions;
using AlgorithmPractice1.Core.Models;

namespace AlgorithmPractice1.Core.Algorithms.Part3_Individual;

/// <summary>
/// Вычисление n-го числа Фибоначчи (итеративный метод, сложность O(n))
/// </summary>
public sealed class FibonacciAlgorithm : AlgorithmBase<int, long>
{
    public override string Id => "Fibonacci";
    public override string DisplayName => "Алгоритм вычисления чисел Фибоначчи";
    public override string Description =>
        "Вычисляет n-е число Фибоначчи итеративным методом. " +
        "F(0)=0, F(1)=1, F(n)=F(n-1)+F(n-2).";
        
    public override AlgorithmCategory Category => AlgorithmCategory.Individual;
    public override MeasurementType MeasurementType => MeasurementType.Time;
    public override ComplexityFunctionType TheoreticalComplexity => ComplexityFunctionType.Linear;

    public override ExperimentConfig DefaultConfig => new()
    {
        NMax = 90,     // Ограничение, чтобы long не переполнился
        NStep = 1,
        RunsPerN = 5
    };

    public override int GenerateTypedInput(int n, ExperimentConfig config) => n;

    public override long ExecuteTyped(int input, MeasurementContext? context)
    {
        if (input <= 1)
        {
            context?.IncrementSteps();
            return input;
        }

        long prev = 0;
        long curr = 1;

        for (int i = 2; i <= input; i++)
        {
            long next = prev + curr;
            prev = curr;
            curr = next;
            context?.IncrementSteps();
        }

        return curr;
    }
}