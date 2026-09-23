namespace AlgorithmPractice1.Core.Models;

/// <summary>
/// Результат аппроксимации методом наименьших квадратов: T(n) = C * f(n).
/// </summary>
public record ApproximationResult
{
    public long Id { get; init; }
    public long SessionAlgorithmId { get; init; }

    /// <summary>
    /// Имя функции сложности (например, "n", "n_log_n", "n2", "n3", "1", "log_n").
    /// </summary>
    public string FunctionType { get; init; } = "n";

    /// <summary>
    /// Коэффициент наклона C.
    /// </summary>
    public double Coefficient { get; init; }

    /// <summary>
    /// Среднеквадратичная ошибка (MSE).
    /// </summary>
    public double Mse { get; init; }

    /// <summary>
    /// Человекочитаемая легенда для графика с форматированным MSE.
    /// </summary>
    public string LegendLabel => FunctionType == "n2_m"
        ? $"Аппроксимация O(n²·m) (MSE: {FormatMse(Mse)})"
        : $"Аппроксимация (MSE: {FormatMse(Mse)})";

    public static string FormatMse(double mse)
    {
        if (double.IsNaN(mse) || double.IsInfinity(mse))
            return "N/A";
        if (Math.Abs(mse) < 1e-4 && Math.Abs(mse) > 0)
            return mse.ToString("E2", System.Globalization.CultureInfo.InvariantCulture);
        return mse.ToString("F6", System.Globalization.CultureInfo.InvariantCulture);
    }
}
