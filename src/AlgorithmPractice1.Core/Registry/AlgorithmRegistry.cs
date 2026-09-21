using AlgorithmPractice1.Core.Abstractions;
using AlgorithmPractice1.Core.Algorithms.Part1_Vectors;
using AlgorithmPractice1.Core.Algorithms.Part2_Matrices;
using AlgorithmPractice1.Core.Algorithms.Part3_Individual;
using AlgorithmPractice1.Core.Algorithms.Part4_Exponentiation;
using AlgorithmPractice1.Core.Models;

namespace AlgorithmPractice1.Core.Registry;

/// <summary>
/// Реестр всех реализованных алгоритмов лабораторной работы.
/// </summary>
public sealed class AlgorithmRegistry
{
    private static readonly Lazy<AlgorithmRegistry> _lazyInstance = new(() => new AlgorithmRegistry());
    public static AlgorithmRegistry Instance => _lazyInstance.Value;

    private readonly Dictionary<string, IAlgorithm> _algorithmsById = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<IAlgorithm> _allAlgorithms = new();

    public IReadOnlyList<IAlgorithm> All => _allAlgorithms;

    public AlgorithmRegistry()
    {
        Register(new ConstantFunctionAlgorithm());
        Register(new VectorSumAlgorithm());
        Register(new VectorProductAlgorithm());
        Register(new NaivePolynomialAlgorithm());
        Register(new HornerPolynomialAlgorithm());
        Register(new BubbleSortAlgorithm());
        Register(new QuickSortAlgorithm());
        Register(new TimSortAlgorithm());

        Register(new MatrixMultiplicationAlgorithm());

        Register(new GnomeSortAlgorithm());
        Register(new PollardRhoAlgorithm());
        Register(new MillerRabinAlgorithm());

        Register(new IterativeExponentiationAlgorithm());
        Register(new RecursiveExponentiationAlgorithm());
        Register(new BinaryExponentiationAlgorithm());
    }

    private void Register(IAlgorithm algorithm)
    {
        _algorithmsById[algorithm.Id] = algorithm;
        _allAlgorithms.Add(algorithm);
    }

    public IAlgorithm? FindById(string id)
    {
        _algorithmsById.TryGetValue(id, out var algorithm);
        return algorithm;
    }

    public IAlgorithm GetById(string id)
    {
        return FindById(id) ?? throw new KeyNotFoundException($"Алгоритм с идентификатором '{id}' не найден в реестре.");
    }

    public IEnumerable<IAlgorithm> GetByCategory(AlgorithmCategory category)
    {
        return _allAlgorithms.Where(a => a.Category == category);
    }
}
