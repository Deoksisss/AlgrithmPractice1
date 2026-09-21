using System.Numerics;
using System.Security.Cryptography;
using AlgorithmPractice1.Core.Abstractions;
using AlgorithmPractice1.Core.Models;

namespace AlgorithmPractice1.Core.Algorithms.Part3_Individual;

/// <summary>
/// 10. Алгоритм Полларда «Ро» (факторизация целых чисел).
/// Параметр n — битовая длина факторизуемого составного числа N = p * q,
/// где p и q — случайные простые числа длины ~n/2 бит каждое.
/// </summary>
public sealed class PollardRhoAlgorithm : AlgorithmBase<BigInteger, BigInteger>
{
    public override string Id => "PollardRho";
    public override string DisplayName => "Алгоритм Полларда «Ро»";
    public override string Description => "Вероятностная факторизация составного числа N = p·q (n — битовая длина N, множители ~n/2 бит). Сложность O(N^(1/4)).";
    public override AlgorithmCategory Category => AlgorithmCategory.Individual;
    public override MeasurementType MeasurementType => MeasurementType.Time;
    public override ComplexityFunctionType TheoreticalComplexity => ComplexityFunctionType.Quadratic;

    public override ExperimentConfig DefaultConfig => new()
    {
        NMax = 44,
        NStep = 4,
        RunsPerN = 5
    };

    public override BigInteger GenerateTypedInput(int n, ExperimentConfig config)
    {
        // Ограничение битовой длины для предотвращения зависания при общих настройках с большими N
        int effectiveN = Math.Min(n, 48);
        int pBits = Math.Max(4, effectiveN / 2);
        int qBits = Math.Max(4, effectiveN - pBits);

        BigInteger p = GeneratePrime(pBits);
        BigInteger q = GeneratePrime(qBits);

        return p * q;
    }

    public override BigInteger ExecuteTyped(BigInteger n, MeasurementContext? context)
    {
        if (n <= 1) return n;
        if (n.IsEven) return 2;

        // Быстрая проверка деления на малые простые числа
        int[] smallPrimes = { 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97 };
        foreach (int sp in smallPrimes)
        {
            if (n % sp == 0) return sp;
        }

        // Высокоэффективная модификация Ричарда Брента для алгоритма Полларда «Ро» с батчевым вычислением НОД
        var rng = new Random(42);
        for (int attempt = 0; attempt < 25; attempt++)
        {
            BigInteger c = NextBigInteger(1, n - 1, rng);
            BigInteger x = NextBigInteger(2, n - 1, rng);
            BigInteger y = x;
            BigInteger d = 1;
            BigInteger q = 1;

            int m = 128; // Размер шага накопления произведения для НОД
            int r = 1;

            BigInteger F(BigInteger val) => ((val * val) + c) % n;

            while (d == 1)
            {
                x = y;
                for (int i = 0; i < r; i++)
                {
                    y = F(y);
                }

                int k = 0;
                while (k < r && d == 1)
                {
                    BigInteger ys = y;
                    int limit = Math.Min(m, r - k);
                    for (int i = 0; i < limit; i++)
                    {
                        y = F(y);
                        BigInteger diff = x > y ? x - y : y - x;
                        q = (q * diff) % n;
                    }

                    d = BigInteger.GreatestCommonDivisor(q, n);
                    k += limit;

                    // Если накопили делитель n (все множители вошли в q), откатываемся и ищем точечно
                    if (d == n)
                    {
                        d = 1;
                        y = ys;
                        while (d == 1)
                        {
                            y = F(y);
                            BigInteger diff = x > y ? x - y : y - x;
                            d = BigInteger.GreatestCommonDivisor(diff, n);
                        }
                    }
                }

                r *= 2;
                if (r > 200000) break; // Защита от слишком долгого цикла на неудачной константе c
            }

            if (d > 1 && d < n)
            {
                return d;
            }
        }

        return FallbackFactor(n);
    }

    private static BigInteger FallbackFactor(BigInteger n)
    {
        int trials = 0;
        for (BigInteger d = 3; d * d <= n; d += 2)
        {
            if (n % d == 0) return d;
            if (++trials > 100000) break;
        }
        return n;
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
            bytes[^1] &= 0x7F; // Гарантируем положительное число
            result = new BigInteger(bytes);
        } while (result >= range);

        return min + result;
    }

    private static BigInteger GeneratePrime(int bits)
    {
        if (bits <= 2) return 3;
        if (bits == 3) return 7;
        if (bits == 4)
        {
            int[] primes4 = { 11, 13 };
            return primes4[Random.Shared.Next(primes4.Length)];
        }

        var rng = new Random();
        while (true)
        {
            byte[] bytes = new byte[(bits + 7) / 8 + 1];
            RandomNumberGenerator.Fill(bytes);
            bytes[^1] = 0;
            var candidate = BigInteger.Abs(new BigInteger(bytes));

            // Устанавливаем старший и младший биты
            candidate |= BigInteger.One << (bits - 1);
            candidate |= BigInteger.One;

            BigInteger mask = (BigInteger.One << bits) - 1;
            candidate &= mask;
            candidate |= BigInteger.One << (bits - 1);
            candidate |= BigInteger.One;

            if (IsProbablePrime(candidate, 10))
            {
                return candidate;
            }
        }
    }

    private static bool IsProbablePrime(BigInteger source, int certainty)
    {
        if (source <= 1) return false;
        if (source == 2 || source == 3) return true;
        if (source.IsEven) return false;

        int[] smallPrimes = { 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37 };
        foreach (int sp in smallPrimes)
        {
            if (source == sp) return true;
            if (source % sp == 0) return false;
        }

        if (source < 37 * 37) return true;

        BigInteger d = source - 1;
        int s = 0;
        while (d.IsEven)
        {
            d /= 2;
            s++;
        }

        var rng = new Random();
        for (int i = 0; i < certainty; i++)
        {
            BigInteger a = NextBigInteger(2, source - 2, rng);
            BigInteger x = BigInteger.ModPow(a, d, source);
            if (x == 1 || x == source - 1)
                continue;

            bool composite = true;
            for (int r = 1; r < s; r++)
            {
                x = BigInteger.ModPow(x, 2, source);
                if (x == source - 1)
                {
                    composite = false;
                    break;
                }
            }

            if (composite) return false;
        }

        return true;
    }
}
