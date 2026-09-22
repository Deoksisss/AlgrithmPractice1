using System.Numerics;
using AlgorithmPractice1.Core.Abstractions;
using AlgorithmPractice1.Core.Models;

namespace AlgorithmPractice1.Core.Algorithms.Part3_Individual;

/// <summary>
/// 10. Алгоритм Полларда «Ро» (факторизация составного числа N = p * q).
/// Использует канонический алгоритм поиска цикла «черепахи и зайца» Флойда.
/// Число шагов пропорционально sqrt(p) = O(N^{1/4}) = O(n).
/// </summary>
public sealed class PollardRhoAlgorithm : AlgorithmBase<BigInteger, BigInteger>
{
    public override string Id => "PollardRho";
    public override string DisplayName => "Алгоритм Полларда «Ро»";
    public override string Description => "Факторизация составного числа N = p·q методом «черепахи и зайца» Флойда. Сложность O(N^{1/4}) = O(n).";
    public override AlgorithmCategory Category => AlgorithmCategory.Individual;
    public override MeasurementType MeasurementType => MeasurementType.Time;
    public override ComplexityFunctionType TheoreticalComplexity => ComplexityFunctionType.Linear;

    public override ExperimentConfig DefaultConfig => new()
    {
        NMax = 150,
        NStep = 25,
        RunsPerN = 3
    };

    public override BigInteger GenerateTypedInput(int n, ExperimentConfig config)
    {
        // Генерируем составное число N = p * q, где наименьший делитель p ~ n^2.
        // Тогда число итераций метода Полларда «Ро» ~ sqrt(p) ~ n, что дает чистое соответствие O(n).
        long targetP = 10000L + (long)n * n * 200;
        long p = NextPrime(targetP);
        long q = NextPrime(p * 3 + 10007);

        return new BigInteger(p) * new BigInteger(q);
    }

    public override BigInteger ExecuteTyped(BigInteger n, MeasurementContext? context)
    {
        if (n <= 1) return n;
        if (n.IsEven) return 2;

        BigInteger x = 2;
        BigInteger y = 2;
        BigInteger c = 1;
        BigInteger d = 1;

        BigInteger F(BigInteger val) => (val * val + c) % n;

        while (d == 1)
        {
            x = F(x);
            y = F(F(y));
            BigInteger diff = x > y ? x - y : y - x;
            d = BigInteger.GreatestCommonDivisor(diff, n);

            if (d == n)
            {
                c++;
                x = 2;
                y = 2;
                d = 1;
            }
        }

        return d;
    }

    private static long NextPrime(long start)
    {
        if (start <= 2) return 2;
        long candidate = start | 1;
        while (!IsPrime(candidate))
        {
            candidate += 2;
        }
        return candidate;
    }

    private static bool IsPrime(long num)
    {
        if (num <= 1) return false;
        if (num <= 3) return true;
        if (num % 2 == 0 || num % 3 == 0) return false;
        for (long i = 5; i * i <= num; i += 6)
        {
            if (num % i == 0 || num % (i + 2) == 0) return false;
        }
        return true;
    }

    public static BigInteger NextBigInteger(BigInteger min, BigInteger max, Random rng)
    {
        if (min >= max) return min;
        BigInteger range = max - min + 1;
        if (range <= int.MaxValue)
        {
            return min + rng.Next((int)range);
        }

        byte[] bytes = range.ToByteArray();
        BigInteger result;
        do
        {
            rng.NextBytes(bytes);
            bytes[^1] &= 0x7F;
            result = new BigInteger(bytes);
        } while (result >= range);

        return min + result;
    }
}
