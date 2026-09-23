using AlgorithmPractice1.Core.Models;

namespace AlgorithmPractice1.Core.Approximation;

/// <summary>
/// Реализация аппроксимации эмпирических данных методом наименьших квадратов (МНК)
/// для однопараметрической модели T_approx(n) = C * f(n) с расчётом среднеквадратичной ошибки (MSE).
/// </summary>
public static class LeastSquaresSolver
{
    public static double EvaluateBasisFunction(ComplexityFunctionType functionType, double n)
    {
        if (n <= 0) n = 1.0;

        return functionType switch
        {
            ComplexityFunctionType.Constant => 1.0,
            ComplexityFunctionType.Linear => n,
            ComplexityFunctionType.Linearithmic => n * Math.Log(n),
            ComplexityFunctionType.Quadratic => n * n,
            ComplexityFunctionType.Cubic => n * n * n,
            ComplexityFunctionType.Logarithmic => Math.Log2(Math.Max(1.0, n)),
            ComplexityFunctionType.Matrix3D => n * n * n,
            _ => n
        };
    }

    /// <summary>
    /// Подбирает коэффициент C и вычисляет MSE для заданного набора точек (n, T_empirical).
    /// Формула: C = sum(T_i * f(n_i)) / sum(f(n_i)^2).
    /// MSE = (1/k) * sum((T_i - C * f(n_i))^2).
    /// </summary>
    public static ApproximationResult Fit(
        IReadOnlyList<(double N, double EmpiricalValue)> points,
        ComplexityFunctionType functionType,
        long sessionAlgorithmId = 0)
    {
        if (points == null || points.Count == 0)
        {
            return new ApproximationResult
            {
                SessionAlgorithmId = sessionAlgorithmId,
                FunctionType = functionType.ToKey(),
                Coefficient = 0.0,
                Mse = 0.0
            };
        }

        double sumNumerator = 0.0;
        double sumDenominator = 0.0;

        foreach (var (n, t) in points)
        {
            double f = EvaluateBasisFunction(functionType, n);
            sumNumerator += t * f;
            sumDenominator += f * f;
        }

        double c = Math.Abs(sumDenominator) > 1e-15 ? sumNumerator / sumDenominator : 0.0;

        // Расчёт MSE
        double sumSquaredError = 0.0;
        foreach (var (n, t) in points)
        {
            double f = EvaluateBasisFunction(functionType, n);
            double predicted = c * f;
            double error = t - predicted;
            sumSquaredError += error * error;
        }

        double mse = sumSquaredError / points.Count;

        return new ApproximationResult
        {
            SessionAlgorithmId = sessionAlgorithmId,
            FunctionType = functionType.ToKey(),
            Coefficient = c,
            Mse = mse
        };
    }

    /// <summary>
    /// Подбирает коэффициент C и вычисляет MSE для двумерного базиса f(n, m) = n^2 * m.
    /// Модель: T_approx(n, m) = C * (n^2 * m).
    /// Формула МНК: C = sum(T_i * n_i^2 * m_i) / sum((n_i^2 * m_i)^2).
    /// MSE = (1/K) * sum((T_i - C * n_i^2 * m_i)^2).
    /// </summary>
    public static ApproximationResult FitMatrix3D(
        IReadOnlyList<(double N, double M, double EmpiricalValue)> points,
        long sessionAlgorithmId = 0)
    {
        if (points == null || points.Count == 0)
        {
            return new ApproximationResult
            {
                SessionAlgorithmId = sessionAlgorithmId,
                FunctionType = ComplexityFunctionType.Matrix3D.ToKey(),
                Coefficient = 0.0,
                Mse = 0.0
            };
        }

        double sumNumerator = 0.0;
        double sumDenominator = 0.0;

        foreach (var (n, m, t) in points)
        {
            double f = n * n * m;
            sumNumerator += t * f;
            sumDenominator += f * f;
        }

        double c = Math.Abs(sumDenominator) > 1e-25 ? sumNumerator / sumDenominator : 0.0;

        double sumSquaredError = 0.0;
        foreach (var (n, m, t) in points)
        {
            double f = n * n * m;
            double predicted = c * f;
            double error = t - predicted;
            sumSquaredError += error * error;
        }

        double mse = sumSquaredError / points.Count;

        return new ApproximationResult
        {
            SessionAlgorithmId = sessionAlgorithmId,
            FunctionType = ComplexityFunctionType.Matrix3D.ToKey(),
            Coefficient = c,
            Mse = mse
        };
    }
}
