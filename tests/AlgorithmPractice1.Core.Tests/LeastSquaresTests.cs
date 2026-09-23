using AlgorithmPractice1.Core.Approximation;
using AlgorithmPractice1.Core.Models;
using Xunit;

namespace AlgorithmPractice1.Core.Tests;

public class LeastSquaresTests
{
    [Fact]
    public void Fit_LinearData_FindsCorrectSlopeAndNearZeroMse()
    {
        // y = 2.5 * n
        var points = new List<(double N, double EmpiricalValue)>
        {
            (1.0, 2.5),
            (2.0, 5.0),
            (3.0, 7.5),
            (4.0, 10.0),
            (5.0, 12.5)
        };

        var result = LeastSquaresSolver.Fit(points, ComplexityFunctionType.Linear);

        Assert.Equal(2.5, result.Coefficient, precision: 6);
        Assert.True(result.Mse < 1e-10);
    }

    [Fact]
    public void Fit_QuadraticData_FindsCorrectCoefficient()
    {
        // y = 0.5 * n^2
        var points = new List<(double N, double EmpiricalValue)>
        {
            (1.0, 0.5),
            (2.0, 2.0),
            (3.0, 4.5),
            (4.0, 8.0)
        };

        var result = LeastSquaresSolver.Fit(points, ComplexityFunctionType.Quadratic);

        Assert.Equal(0.5, result.Coefficient, precision: 6);
        Assert.True(result.Mse < 1e-10);
    }

    [Fact]
    public void Fit_ConstantData_FindsCorrectConstant()
    {
        // y = 7.0
        var points = new List<(double N, double EmpiricalValue)>
        {
            (10.0, 7.0),
            (20.0, 7.0),
            (30.0, 7.0)
        };

        var result = LeastSquaresSolver.Fit(points, ComplexityFunctionType.Constant);

        Assert.Equal(7.0, result.Coefficient, precision: 6);
        Assert.True(result.Mse < 1e-10);
    }

    [Fact]
    public void FormatMse_FormatsSmallValuesCorrectly()
    {
        double mse = 1.23e-6;
        string formatted = ApproximationResult.FormatMse(mse);
        Assert.Contains("E", formatted);
    }

    [Fact]
    public void FitMatrix3D_SyntheticN2M_FindsExactCoefficientAndZeroMse()
    {
        // T(n, m) = 1.5e-6 * (n^2 * m)
        double expectedC = 1.5e-6;
        var points = new List<(double N, double M, double EmpiricalValue)>();
        int[] ns = { 10, 20, 30, 40 };
        int[] ms = { 10, 20, 30, 40 };

        foreach (var n in ns)
        {
            foreach (var m in ms)
            {
                points.Add((n, m, expectedC * (n * n * m)));
            }
        }

        var result = LeastSquaresSolver.FitMatrix3D(points, 42);

        Assert.Equal(42, result.SessionAlgorithmId);
        Assert.Equal("n2_m", result.FunctionType);
        Assert.Equal(expectedC, result.Coefficient, precision: 12);
        Assert.True(result.Mse < 1e-12);
        Assert.Contains("O(n²·m)", result.LegendLabel);
    }
}

