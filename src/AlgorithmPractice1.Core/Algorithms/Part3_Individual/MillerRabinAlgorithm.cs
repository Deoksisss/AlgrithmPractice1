using System.Numerics;
using System.Security.Cryptography;
using AlgorithmPractice1.Core.Abstractions;
using AlgorithmPractice1.Core.Models;

namespace AlgorithmPractice1.Core.Algorithms.Part3_Individual;

public record MillerRabinInput(BigInteger Number, int Rounds);

/// <summary>
/// 11. Тест простоты Миллера–Рабина.
/// Параметр n — битовая длина тестируемого нечётного числа.
/// Параметр k — количество независимых раундов проверки.
/// </summary>
public sealed class MillerRabinAlgorithm : AlgorithmBase<MillerRabinInput, bool>
{
    public const int DefaultRounds = 20;

    public override string Id => "MillerRabin";
    public override string DisplayName => "Тест Миллера–Рабина";
    public override string Description => "Вероятностный тест простоты (n — битовая длина числа, k — число раундов). Сложность O(k·n³).";
    public override AlgorithmCategory Category => AlgorithmCategory.Individual;
    public override MeasurementType MeasurementType => MeasurementType.Time;
    public override ComplexityFunctionType TheoreticalComplexity => ComplexityFunctionType.Cubic;

    public override ExperimentConfig DefaultConfig => new()
    {
        NMax = 1024,
        NStep = 128,
        RunsPerN = 5,
        K = DefaultRounds
    };

    public override MillerRabinInput GenerateTypedInput(int n, ExperimentConfig config)
    {
        int rounds = config.K.GetValueOrDefault(DefaultRounds);
        if (rounds <= 0) rounds = DefaultRounds;

        int bits = Math.Max(8, n);
        byte[] bytes = new byte[(bits + 7) / 8 + 1];
        RandomNumberGenerator.Fill(bytes);
        bytes[^1] = 0;

        var num = BigInteger.Abs(new BigInteger(bytes));
        // Устанавливаем старший бит нужного порядка и младший бит (нечётное)
        num |= BigInteger.One << (bits - 1);
        num |= BigInteger.One;

        BigInteger mask = (BigInteger.One << bits) - 1;
        num &= mask;
        num |= BigInteger.One << (bits - 1);
        num |= BigInteger.One;

        return new MillerRabinInput(num, rounds);
    }

    public override bool ExecuteTyped(MillerRabinInput input, MeasurementContext? context)
    {
        BigInteger n = input.Number;
        int rounds = input.Rounds;

        if (n == 2 || n == 3) return true;
        if (n < 2 || n.IsEven) return false;

        BigInteger d = n - 1;
        int s = 0;
        while (d.IsEven)
        {
            d /= 2;
            s++;
        }

        var rng = new Random(42);

        for (int i = 0; i < rounds; i++)
        {
            BigInteger a = RandomBase(n, rng);

            BigInteger x = BigInteger.ModPow(a, d, n);
            if (x == 1 || x == n - 1)
                continue;

            bool composite = true;
            for (int r = 1; r < s; r++)
            {
                x = BigInteger.ModPow(x, 2, n);
                if (x == n - 1)
                {
                    composite = false;
                    break;
                }
            }

            if (composite)
                return false;
        }

        return true;
    }

    private static BigInteger RandomBase(BigInteger max, Random rng)
    {
        byte[] bytes = max.ToByteArray();
        rng.NextBytes(bytes);
        bytes[^1] &= 0x7F;
        return (new BigInteger(bytes) % (max - 3)) + 2;
    }
}
