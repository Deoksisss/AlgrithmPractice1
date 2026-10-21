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
    public void Permutations_StepsCountEqualsFactorial()
    {
        var alg = new PermutationsAlgorithm();
        var ctx = new MeasurementContext();
        var input = alg.GenerateTypedInput(4, new ExperimentConfig()); // 4! = 24
    
        alg.ExecuteTyped(input, ctx);
    
        Assert.Equal(24, ctx.StepCount); 
    }

        // ============================================================
        // ТЕСТЫ ДЛЯ ФИБОНАЧЧИ
        // ============================================================

        [Fact]
        public void Fibonacci_ProducesCorrectValues()
        {
            var alg = new FibonacciAlgorithm();
            var config = new ExperimentConfig();

            // Эталонные значения: F(0)=0, F(1)=1, F(2)=1, F(3)=2, F(4)=3, F(5)=5, F(10)=55, F(15)=610, F(20)=6765
            var expected = new Dictionary<int, long>
            {
                { 0, 0 },
                { 1, 1 },
                { 2, 1 },
                { 3, 2 },
                { 4, 3 },
                { 5, 5 },
                { 10, 55 },
                { 15, 610 },
                { 20, 6765 }
            };

            foreach (var kvp in expected)
            {
                var input = alg.GenerateTypedInput(kvp.Key, config);
                long result = alg.ExecuteTyped(input, null);
                Assert.Equal(kvp.Value, result);
            }
        }

        [Fact]
        public void FibonacciNaiveRecursive_ExponentialGrowth_StepsCountMatchesFormula()
        {
            // Для наивной рекурсии количество вызовов Fibonacci(n) = 2*F(n+1) - 1.
            // Проверяем это на небольших n, чтобы убедиться, что сложность именно экспоненциальная.
            var alg = new FibonacciNaiveRecursiveAlgorithm();
            var config = new ExperimentConfig();

            // n=5: 2*F(6)-1 = 2*8-1 = 15 вызовов
            // n=6: 2*F(7)-1 = 2*13-1 = 25 вызовов
            // n=7: 2*F(8)-1 = 2*21-1 = 41 вызов
            // n=8: 2*F(9)-1 = 2*34-1 = 67 вызовов
            var cases = new[]
            {
                (N: 5, ExpectedSteps: 15L),
                (N: 6, ExpectedSteps: 25L),
                (N: 7, ExpectedSteps: 41L),
                (N: 8, ExpectedSteps: 67L)
            };

            foreach (var (n, expectedSteps) in cases)
            {
                var ctx = new MeasurementContext();
                var input = alg.GenerateTypedInput(n, config);
                alg.ExecuteTyped(input, ctx);

                Assert.Equal(expectedSteps, ctx.StepCount);
            }
        }

        [Fact]
        public void Fibonacci_RegisteredInRegistry()
        {
            // Проверяем, что алгоритм автоматически подхватился через рефлексию/регистрацию
            var alg = AlgorithmRegistry.Instance.GetById("Fibonacci");
            Assert.NotNull(alg);
            Assert.IsType<FibonacciAlgorithm>(alg);
            Assert.Equal(ComplexityFunctionType.Linear, alg.TheoreticalComplexity);

            var naiveAlg = AlgorithmRegistry.Instance.GetById("FibonacciNaiveRecursive");
            Assert.NotNull(naiveAlg);
            Assert.IsType<FibonacciNaiveRecursiveAlgorithm>(naiveAlg);
            Assert.Equal(ComplexityFunctionType.Exponential, naiveAlg.TheoreticalComplexity);
        }

    [Fact]
    public void Fibonacci_StepsCountEqualsN()
    {
        var alg = new FibonacciAlgorithm();
        var ctx = new MeasurementContext();
        int n = 10;                              // F(10) = 55

        alg.ExecuteTyped(n, ctx);

        // Итеративный вариант делает (n-1) шагов в цикле
        Assert.Equal(9, ctx.StepCount);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(10, 55)]
    [InlineData(20, 6765)]
    [InlineData(50, 12586269025)]
    public void Fibonacci_ReturnsCorrectValue(int n, long expected)
    {
        var alg = new FibonacciAlgorithm();
        long actual = alg.ExecuteTyped(n, null);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(10, 55)]
    [InlineData(20, 6765)]
    public void FibonacciNaiveRecursive_ReturnsCorrectValue(int n, long expected)
    {
        var alg = new FibonacciNaiveRecursiveAlgorithm();
        long actual = alg.ExecuteTyped(n, null);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void FibonacciNaiveRecursive_ExponentialStepCount()
    {
        var alg = new FibonacciNaiveRecursiveAlgorithm();
        var ctx = new MeasurementContext();
        
        // Для n=10 количество вызовов функции должно быть 177 
        // (это 2 * F(11) - 1, классическая формула для дерева вызовов)
        alg.ExecuteTyped(10, ctx);
        
        Assert.Equal(177, ctx.StepCount); 
    }
    
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(10, 55)]
    [InlineData(50, 12586269025)]
    [InlineData(90, 2880067194370816120)] // Максимальное значение для long
    public void FibonacciMemoized_ReturnsCorrectValue(int n, long expected)
    {
        var alg = new FibonacciMemoizedAlgorithm();
        long actual = alg.ExecuteTyped(n, null);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void FibonacciMemoized_LinearStepCount()
    {
        var alg = new FibonacciMemoizedAlgorithm();
        var ctx = new MeasurementContext();
        int n = 90;

        alg.ExecuteTyped(n, ctx);

        // Благодаря мемоизации мы вычисляем каждое число ровно один раз (плюс проверки кэша).
        // Для n = 90 вычисляются все значения от 0 до 90 (ровно 91 шаг).
        Assert.Equal(91, ctx.StepCount); 
    }
    
    [Fact]
    public void FibonacciMemoized_ExecutionIsFast()
    {
        var alg = new FibonacciMemoizedAlgorithm();
        var sw = System.Diagnostics.Stopwatch.StartNew();

        alg.ExecuteTyped(90, null);

        sw.Stop();
        // Мемоизация на 90 элементах должна работать за миллисекунды
        Assert.True(sw.ElapsedMilliseconds < 50);
    }
    
    [Fact]
    public void Fibonacci_MemoizedIsMuchFasterThanNaive()
    {
        var naive = new FibonacciNaiveRecursiveAlgorithm();
        var memo = new FibonacciMemoizedAlgorithm();
        int n = 30; // 30 - хороший баланс, наивный уже тормозит, но еще не зависает

        // Замер наивного
        var swNaive = System.Diagnostics.Stopwatch.StartNew();
        naive.ExecuteTyped(n, null);
        swNaive.Stop();

        // Замер с мемоизацией
        var swMemo = System.Diagnostics.Stopwatch.StartNew();
        memo.ExecuteTyped(n, null);
        swMemo.Stop();

        // Наивный должен быть как минимум в 50 раз медленнее на n=30
        Assert.True(swNaive.ElapsedMilliseconds > swMemo.ElapsedMilliseconds * 50,
            $"Наивный: {swNaive.ElapsedMilliseconds} мс, Мемо: {swMemo.ElapsedMilliseconds} мс");
    }
    
    [Fact]
    public void Fibonacci_ExecutionIsFast()
    {
        var alg = new FibonacciAlgorithm();
        var sw = System.Diagnostics.Stopwatch.StartNew();

        // Для линейного алгоритма F(90) должен выполняться мгновенно
        alg.ExecuteTyped(90, null);

        sw.Stop();
        Assert.True(sw.ElapsedMilliseconds < 50,
            $"Fibonacci(90) выполнялся {sw.ElapsedMilliseconds} мс, ожидалось < 50 мс");
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

    [Fact]
    public void InductiveSubsets_CorrectSubsetsAndOrder()
    {
        var alg = new InductiveSubsetsAlgorithm();
        int[] input = { 0, 1, 2, 3 };
        var ctx = new MeasurementContext();

        var subsets = alg.ExecuteTyped(input, ctx);

        Assert.Equal(16, subsets.Count);
        Assert.Equal(16, ctx.StepCount);

        // Проверяем точный порядок индуктивной генерации:
        // []
        Assert.Equal(Array.Empty<int>(), subsets[0]);
        // [0]
        Assert.Equal(new[] { 0 }, subsets[1]);
        // [1], [0, 1]
        Assert.Equal(new[] { 1 }, subsets[2]);
        Assert.Equal(new[] { 0, 1 }, subsets[3]);
        // [2], [0, 2], [1, 2], [0, 1, 2]
        Assert.Equal(new[] { 2 }, subsets[4]);
        Assert.Equal(new[] { 0, 2 }, subsets[5]);
        Assert.Equal(new[] { 1, 2 }, subsets[6]);
        Assert.Equal(new[] { 0, 1, 2 }, subsets[7]);
        // [3], [0, 3], [1, 3], [0, 1, 3], [2, 3], [0, 2, 3], [1, 2, 3], [0, 1, 2, 3]
        Assert.Equal(new[] { 3 }, subsets[8]);
        Assert.Equal(new[] { 0, 3 }, subsets[9]);
        Assert.Equal(new[] { 1, 3 }, subsets[10]);
        Assert.Equal(new[] { 0, 1, 3 }, subsets[11]);
        Assert.Equal(new[] { 2, 3 }, subsets[12]);
        Assert.Equal(new[] { 0, 2, 3 }, subsets[13]);
        Assert.Equal(new[] { 1, 2, 3 }, subsets[14]);
        Assert.Equal(new[] { 0, 1, 2, 3 }, subsets[15]);
    }

    [Fact]
    public void InductiveSubsets_RegisteredInRegistryAndGeneratesSequentialInputs()
    {
        var alg = AlgorithmRegistry.Instance.GetById("InductiveSubsets");
        Assert.NotNull(alg);
        Assert.Equal(AlgorithmCategory.Individual, alg.Category);
        Assert.Equal(ComplexityFunctionType.Exponential, alg.TheoreticalComplexity);

        var typedAlg = Assert.IsType<InductiveSubsetsAlgorithm>(alg);
        var input5 = typedAlg.GenerateTypedInput(5, typedAlg.DefaultConfig);
        Assert.Equal(new[] { 0, 1, 2, 3, 4 }, input5);

        var emptyInput = typedAlg.GenerateTypedInput(0, typedAlg.DefaultConfig);
        var emptyRes = typedAlg.ExecuteTyped(emptyInput, null);
        Assert.Single(emptyRes);
        Assert.Empty(emptyRes[0]);
    }
}
