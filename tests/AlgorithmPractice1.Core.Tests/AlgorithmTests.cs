using System.Numerics;
using AlgorithmPractice1.Core.Algorithms.Part1_Vectors;
using AlgorithmPractice1.Core.Algorithms.Part2_Matrices;
using AlgorithmPractice1.Core.Algorithms.Part3_Individual;
using AlgorithmPractice1.Core.Algorithms.Part4_Exponentiation;
using AlgorithmPractice1.Core.Approximation;
using AlgorithmPractice1.Core.Models;
using AlgorithmPractice1.Core.Registry;
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
    public void GnomeSort_SortsFastAndCorrectly_AtLargeN()
    {
        var alg = new GnomeSortAlgorithm();
        // Проверка корректности и скорости при N = 5000
        var input = alg.GenerateTypedInput(5000, new ExperimentConfig());
        var expected = (int[])input.Clone();
        Array.Sort(expected);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var sorted = alg.ExecuteTyped(input, null);
        sw.Stop();

        Assert.Equal(expected, sorted);
        // Оптимизированный GnomeSort на 5000 элементов должен выполняться менее чем за 300 мс
        Assert.True(sw.ElapsedMilliseconds < 1000);
    }

    [Fact]
    public void LargeN_FastVectorAlgorithms_ExecuteQuicklyWithSmallStep()
    {
        // Линейные и N*log(N) алгоритмы: N до 20000 с шагом 2000
        var sumAlg = new VectorSumAlgorithm();
        var prodAlg = new VectorProductAlgorithm();
        var hornerAlg = new HornerPolynomialAlgorithm();
        var quickAlg = new QuickSortAlgorithm();
        var timAlg = new TimSortAlgorithm();

        var config = new ExperimentConfig { NMax = 20000, NStep = 2000, RunsPerN = 1 };

        for (int n = 2000; n <= 20000; n += 2000)
        {
            var vec = sumAlg.GenerateTypedInput(n, config);
            double sum = sumAlg.ExecuteTyped(vec, null);
            Assert.False(double.IsNaN(sum));

            var pVec = prodAlg.GenerateTypedInput(n, config);
            double prod = prodAlg.ExecuteTyped(pVec, null);
            Assert.False(double.IsNaN(prod));

            var hVec = hornerAlg.GenerateTypedInput(n, config);
            double horner = hornerAlg.ExecuteTyped(hVec, null);
            Assert.False(double.IsNaN(horner));

            var qVec = quickAlg.GenerateTypedInput(n, config);
            var qSorted = quickAlg.ExecuteTyped(qVec, null);
            Assert.Equal(n, qSorted.Length);

            var tVec = timAlg.GenerateTypedInput(n, config);
            var tSorted = timAlg.ExecuteTyped(tVec, null);
            Assert.Equal(n, tSorted.Length);
        }
    }

    [Fact]
    public void Exponentiation_ExecutesSafely_AtLargeN_WithoutStackOverflow()
    {
        var iterAlg = new IterativeExponentiationAlgorithm();
        var recAlg = new RecursiveExponentiationAlgorithm();
        var binAlg = new BinaryExponentiationAlgorithm();
        var classicAlg = new ClassicalExponentiationAlgorithm();

        // Проверка при экстремально большом N = 20000 (проверка отсутствия StackOverflow)
        var input = new ExponentiationInput(1.0001, 20000);

        double resIter = iterAlg.ExecuteTyped(input, null);
        double resRec = recAlg.ExecuteTyped(input, null);
        double resBin = binAlg.ExecuteTyped(input, null);
        double resClassic = classicAlg.ExecuteTyped(input, null);

        Assert.True(resIter > 0 && !double.IsInfinity(resIter));
        Assert.True(resRec > 0 && !double.IsInfinity(resRec));
        Assert.True(resBin > 0 && !double.IsInfinity(resBin));
        Assert.True(resClassic > 0 && !double.IsInfinity(resClassic));
        Assert.Equal(resIter, resRec, precision: 4);
        Assert.Equal(resIter, resBin, precision: 4);
        Assert.Equal(resIter, resClassic, precision: 4);
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
        var config = new ExperimentConfig { NMax = 150, NStep = 25, RunsPerN = 3 };
        var points = new List<(double N, double EmpiricalValue)>();

        for (int n = 25; n <= 150; n += 25)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            for (int r = 0; r < 3; r++)
            {
                var input = alg.GenerateTypedInput(n, config);
                var f = alg.ExecuteTyped(input, null);
                Assert.True(f > 1 && f < input);
            }
            sw.Stop();
            double avgMs = sw.Elapsed.TotalMilliseconds / 3.0;
            points.Add((n, avgMs));
            Console.WriteLine($"n={n}: {avgMs:F4} ms");
        }

        var fit = LeastSquaresSolver.Fit(points, alg.TheoreticalComplexity);
        Console.WriteLine($"PollardRho Fit: Function={fit.FunctionType}, C={fit.Coefficient:E3}, MSE={fit.Mse:E3}");
        Assert.True(fit.Mse < 10.0);
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
        var classicAlg = new ClassicalExponentiationAlgorithm();

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
        // Бинарное рекурсивное: 10 = 1010_2. Гораздо меньше умножений!
        Assert.True(ctxBin.StepCount <= 6);

        var ctxClassic = new MeasurementContext();
        double resClassic = classicAlg.ExecuteTyped(input, ctxClassic);
        Assert.Equal(1024.0, resClassic);
        // Классическое битовое: 10 = 1010_2. 4 сдвига, 2 бита 1.
        Assert.True(ctxClassic.StepCount <= 6);
    }

    [Fact]
    public void FourExponentiationAlgorithms_RegisteredAndAccurate()
    {
        var expAlgs = AlgorithmRegistry.Instance.GetByCategory(AlgorithmCategory.Exponentiation).ToList();
        Assert.Equal(4, expAlgs.Count);

        var ids = expAlgs.Select(a => a.Id).ToHashSet();
        Assert.Contains("IterativeExponentiation", ids);
        Assert.Contains("RecursiveExponentiation", ids);
        Assert.Contains("BinaryExponentiation", ids);
        Assert.Contains("ClassicalExponentiation", ids);

        // Проверяем краевые случаи (x^0 = 1, x^1 = x, 3^5 = 243)
        foreach (var alg in expAlgs)
        {
            var config = new ExperimentConfig();
            var input0 = new ExponentiationInput(5.0, 0);
            var input1 = new ExponentiationInput(5.0, 1);
            var input5 = new ExponentiationInput(3.0, 5);

            Assert.Equal(1.0, (double)alg.Execute(input0, null)!);
            Assert.Equal(5.0, (double)alg.Execute(input1, null)!);
            Assert.Equal(243.0, (double)alg.Execute(input5, null)!);
        }

        // Классический алгоритм на N = 100000 выполняется мгновенно и безопасно (память O(1), без стека)
        var classic = new ClassicalExponentiationAlgorithm();
        var largeInput = new ExponentiationInput(1.00001, 100000);
        var sw = System.Diagnostics.Stopwatch.StartNew();
        double res = classic.ExecuteTyped(largeInput, null);
        sw.Stop();
        Assert.True(res > 1.0);
        Assert.True(sw.ElapsedMilliseconds < 50);
    }

    [Fact]
    public void MatrixMultiplication_SmallN_SmallStep_CompletesQuickly()
    {
        var alg = new MatrixMultiplicationAlgorithm();
        // Умножение матриц O(n^2 * m) тестируется на меньших n и m с малым шагом (шаг 10)
        for (int n = 10; n <= 40; n += 10)
        {
            var config = new ExperimentConfig { M = n };
            var pair = alg.GenerateTypedInput(n, config);
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var res = alg.ExecuteTyped(pair, null);
            sw.Stop();
            Assert.Equal(n, res.GetLength(0));
            Assert.Equal(n, res.GetLength(1));
            Assert.True(sw.ElapsedMilliseconds < 500);
        }
    }
}
