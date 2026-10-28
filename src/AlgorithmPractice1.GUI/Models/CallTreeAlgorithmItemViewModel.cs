using AlgorithmPractice1.Core.Abstractions;
using AlgorithmPractice1.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AlgorithmPractice1.GUI.Models;

/// <summary>
/// Элемент выбора рекурсивного алгоритма для отображения дерева вызовов.
/// </summary>
public partial class CallTreeAlgorithmItemViewModel : ObservableObject
{
    public IAlgorithm Algorithm { get; }

    public string DisplayName => Algorithm.DisplayName;
    public string Id => Algorithm.Id;
    public string ComplexityDisplayName => Algorithm.TheoreticalComplexity.ToDisplayName();

    [ObservableProperty]
    private bool _isSelected;

    public int SuggestedN => Algorithm.Id switch
    {
        "RTreeSearch" => 20,
        "InductiveSubsets" => 4,
        "Permutations" => 4,
        "FibonacciNaiveRecursive" => 5,
        "FibonacciMemoized" => 6,
        "RecursiveExponentiation" => 4,
        "BinaryExponentiation" => 8,
        "QuickSort" => 6,
        _ => 4
    };

    public int MaxAllowedN => Algorithm.Id switch
    {
        "RTreeSearch" => 100,
        "InductiveSubsets" => 10,   // 2^10 = 1024 вызовов/подмножеств
        "Permutations" => 6,        // 6! = 720 вызовов (безопасно для UI)
        "FibonacciNaiveRecursive" => 8, // 2*F(9)-1 = 67 вызовов
        "FibonacciMemoized" => 20,
        "RecursiveExponentiation" => 15,
        "BinaryExponentiation" => 32,
        "QuickSort" => 16,
        _ => 10
    };

    public CallTreeAlgorithmItemViewModel(IAlgorithm algorithm)
    {
        Algorithm = algorithm;
    }
}
