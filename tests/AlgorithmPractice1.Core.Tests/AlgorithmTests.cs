using System.Numerics;
using AlgorithmPractice1.Core.Algorithms.Part1_Vectors;
using AlgorithmPractice1.Core.Algorithms.Part2_Matrices;
using AlgorithmPractice1.Core.Algorithms.Part3_Individual;
using AlgorithmPractice1.Core.Algorithms.Part4_Exponentiation;
using AlgorithmPractice1.Core.Models;
using Xunit;

namespace AlgorithmPractice1.Core.Tests;

public class AlgorithmTests
{
    [Fact]
    public void ConstantFunction_AlwaysReturnsOne()
    {
        var alg = new ConstantFunctionAlgorithm();
        var input = alg.GenerateTypedInput(100, alg.DefaultConfig);
        double result = alg.ExecuteTyped(input, null);
        Assert.Equal(1.0, result);
    }

    [Fact]
    public void VectorSum_ComputesSumCorrectly()
    {
        var alg = new VectorSumAlgorithm();
        double[] input = { 1.5, 2.5, 3.0, 4.0 };
        double sum = alg.ExecuteTyped(input, null);
        Assert.Equal(11.0, sum);
    }

    [Fact]
    public void VectorProduct_ComputesProductCorrectly()
    {
        var alg = new VectorProductAlgorithm();
        double[] input = { 2.0, 3.0, 4.0 };
        double prod = alg.ExecuteTyped(input, null);
        Assert.Equal(24.0, prod);
    }

    [Fact]
    public void NaiveAndHornerPolynomials_ProduceIdenticalResults()
    {
        var naive = new NaivePolynomialAlgorithm();
        var horner = new HornerPolynomialAlgorithm();

        double[] coeffs = { 2.0, -1.0, 3.0, 0.5 }; // P(x) = 2 - x + 3x^2 + 0.5x^3
        double resNaive = naive.ExecuteTyped(coeffs, null);
        double resHorner = horner.ExecuteTyped(coeffs, null);

        Assert.Equal(resNaive, resHorner, precision: 6);
    }

    [Theory]
    [InlineData("BubbleSort")]
    [InlineData("QuickSort")]
    [InlineData("TimSort")]
    [InlineData("GnomeSort")]
    public void SortingAlgorithms_CorrectlySortRandomArray(string algId)
    {
        int[] original = { 5, 2, 8, 1, 9, 3, 7, 4, 6, 0, -2, 10 };
        int[] expected = (int[])original.Clone();
        Array.Sort(expected);

        int[] result;
        switch (algId)
        {
            case "BubbleSort":
                result = new BubbleSortAlgorithm().ExecuteTyped(original, null);
                break;
            case "QuickSort":
                result = new QuickSortAlgorithm().ExecuteTyped(original, null);
                break;
            case "TimSort":
                result = new TimSortAlgorithm().ExecuteTyped(original, null);
                break;
            case "GnomeSort":
                result = new GnomeSortAlgorithm().ExecuteTyped(original, null);
                break;
            default:
                throw new ArgumentException(algId);
        }

        Assert.Equal(expected, result);
    }

    [Fact]
    public void MatrixMultiplication_MultipliesCorrectly()
    {
        var alg = new MatrixMultiplicationAlgorithm();
        // A: 2x3, B: 3x2
        var a = new double[,]
        {
            { 1, 2, 3 },
            { 4, 5, 6 }
        };
        var b = new double[,]
        {
            { 7, 8 },
            { 9, 1 },
            { 2, 3 }
        };

        var pair = new MatrixPair(a, b, 2, 3);
        var c = alg.ExecuteTyped(pair, null);

        // c[0,0] = 1*7 + 2*9 + 3*2 = 7 + 18 + 6 = 31
        // c[0,1] = 1*8 + 2*1 + 3*3 = 8 + 2 + 9 = 19
        // c[1,0] = 4*7 + 5*9 + 6*2 = 28 + 45 + 12 = 85
        // c[1,1] = 4*8 + 5*1 + 6*3 = 32 + 5 + 18 = 55
        Assert.Equal(31, c[0, 0]);
        Assert.Equal(19, c[0, 1]);
        Assert.Equal(85, c[1, 0]);
        Assert.Equal(55, c[1, 1]);
    }

    [Fact]
    public void PollardRho_FactorsCompositeNumber()
    {
        var alg = new PollardRhoAlgorithm();
        // N = 8051 = 83 * 97
        var n = new BigInteger(8051);
        var factor = alg.ExecuteTyped(n, null);

        Assert.True(factor > 1 && factor < n);
        Assert.True(n % factor == 0);
    }

    [Fact]
    public void PollardRho_TimingCheck()
    {
        var alg = new PollardRhoAlgorithm();
        var config = alg.DefaultConfig;
        var sw = new System.Diagnostics.Stopwatch();

        for (int n = 4; n <= 44; n += 4)
        {
            sw.Restart();
            for (int r = 0; r < 3; r++)
            {
                var input = alg.GenerateTypedInput(n, config);
                var f = alg.ExecuteTyped(input, null);
                Assert.True(f > 1 && f <= input);
            }
            sw.Stop();
            System.Diagnostics.Trace.WriteLine($"n={n}: {sw.ElapsedMilliseconds} ms for 3 runs");
        }
    }

    [Fact]
    public void MillerRabin_IdentifiesPrimesAndComposites()
    {
        var alg = new MillerRabinAlgorithm();

        // 104729 - известное простое число
        var primeInput = new MillerRabinInput(new BigInteger(104729), 20);
        Assert.True(alg.ExecuteTyped(primeInput, null));

        // 8051 = 83 * 97 - составное
        var compositeInput = new MillerRabinInput(new BigInteger(8051), 20);
        Assert.False(alg.ExecuteTyped(compositeInput, null));
    }

    [Fact]
    public void ExponentiationAlgorithms_CalculatePowersAndCountSteps()
    {
        var iterAlg = new IterativeExponentiationAlgorithm();
        var recAlg = new RecursiveExponentiationAlgorithm();
        var binAlg = new BinaryExponentiationAlgorithm();

        var input = new ExponentiationInput(2.0, 10); // 2^10 = 1024

        var ctxIter = new MeasurementContext();
        double resIter = iterAlg.ExecuteTyped(input, ctxIter);
        Assert.Equal(1024.0, resIter);
        Assert.Equal(9, ctxIter.StepCount); // 9 умножений

        var ctxRec = new MeasurementContext();
        double resRec = recAlg.ExecuteTyped(input, ctxRec);
        Assert.Equal(1024.0, resRec);
        Assert.Equal(9, ctxRec.StepCount); // 9 умножений

        var ctxBin = new MeasurementContext();
        double resBin = binAlg.ExecuteTyped(input, ctxBin);
        Assert.Equal(1024.0, resBin);
        // Бинарное: 10 = 1010_2. Гораздо меньше умножений!
        Assert.True(ctxBin.StepCount <= 6);
    }
}
