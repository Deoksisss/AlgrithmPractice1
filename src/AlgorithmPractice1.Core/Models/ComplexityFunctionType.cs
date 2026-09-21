namespace AlgorithmPractice1.Core.Models;

/// <summary>
/// Тип теоретической базисной функции сложности f(n) для МНК-аппроксимации.
/// </summary>
public enum ComplexityFunctionType
{
    /// <summary>
    /// f(n) = 1 (O(1))
    /// </summary>
    Constant,

    /// <summary>
    /// f(n) = n (O(n))
    /// </summary>
    Linear,

    /// <summary>
    /// f(n) = n * ln(n) (O(n log n))
    /// </summary>
    Linearithmic,

    /// <summary>
    /// f(n) = n^2 (O(n^2))
    /// </summary>
    Quadratic,

    /// <summary>
    /// f(n) = n^3 (O(n^3))
    /// </summary>
    Cubic,

    /// <summary>
    /// f(n) = log2(n) (O(log n))
    /// </summary>
    Logarithmic
}

public static class ComplexityFunctionExtensions
{
    public static string ToKey(this ComplexityFunctionType type) => type switch
    {
        ComplexityFunctionType.Constant => "1",
        ComplexityFunctionType.Linear => "n",
        ComplexityFunctionType.Linearithmic => "n_log_n",
        ComplexityFunctionType.Quadratic => "n2",
        ComplexityFunctionType.Cubic => "n3",
        ComplexityFunctionType.Logarithmic => "log_n",
        _ => "n"
    };

    public static ComplexityFunctionType FromKey(string key) => key switch
    {
        "1" => ComplexityFunctionType.Constant,
        "n" => ComplexityFunctionType.Linear,
        "n_log_n" => ComplexityFunctionType.Linearithmic,
        "n2" => ComplexityFunctionType.Quadratic,
        "n3" => ComplexityFunctionType.Cubic,
        "log_n" => ComplexityFunctionType.Logarithmic,
        _ => ComplexityFunctionType.Linear
    };

    public static string ToDisplayName(this ComplexityFunctionType type) => type switch
    {
        ComplexityFunctionType.Constant => "O(1)",
        ComplexityFunctionType.Linear => "O(n)",
        ComplexityFunctionType.Linearithmic => "O(n log n)",
        ComplexityFunctionType.Quadratic => "O(n²)",
        ComplexityFunctionType.Cubic => "O(n³)",
        ComplexityFunctionType.Logarithmic => "O(log n)",
        _ => "O(n)"
    };
}
