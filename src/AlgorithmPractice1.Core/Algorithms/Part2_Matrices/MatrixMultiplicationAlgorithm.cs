using AlgorithmPractice1.Core.Abstractions;
using AlgorithmPractice1.Core.Models;

namespace AlgorithmPractice1.Core.Algorithms.Part2_Matrices;

public record MatrixPair(double[,] A, double[,] B, int N, int M);

/// <summary>
/// 8. Классическое умножение матриц A (n x m) * B (m x n) (сложность O(n * m * n)).
/// Варьируются оба размера n и m на двумерной сетке. Визуализируется на 3D графике (n, m, время).
/// </summary>
public sealed class MatrixMultiplicationAlgorithm : AlgorithmBase<MatrixPair, double[,]>
{
    public const int DefaultMMax = 150;
    public const int DefaultMStep = 25;

    public override string Id => "MatrixMultiply";
    public override string DisplayName => "Умножение матриц (A: n×m × B: m×n)";
    public override string Description => "Классическое умножение прямоугольных матриц (O(n²·m)). Варьируются оба размера n и m. Результат отображается на 3D графике (n, m, время).";
    public override AlgorithmCategory Category => AlgorithmCategory.Matrices;
    public override MeasurementType MeasurementType => MeasurementType.Time;
    public override ComplexityFunctionType TheoreticalComplexity => ComplexityFunctionType.Matrix3D;

    public override ExperimentConfig DefaultConfig => new()
    {
        NMax = 150,
        NStep = 25,
        RunsPerN = 3,
        M = DefaultMMax,
        MStep = DefaultMStep
    };

    public override MatrixPair GenerateTypedInput(int n, ExperimentConfig config)
    {
        int m = config.M.GetValueOrDefault(n);
        if (m <= 0) m = n;

        var random = new Random(42 + n + m);
        var a = new double[n, m];
        var b = new double[m, n];

        for (int i = 0; i < n; i++)
        {
            for (int k = 0; k < m; k++)
            {
                a[i, k] = random.NextDouble();
            }
        }

        for (int k = 0; k < m; k++)
        {
            for (int j = 0; j < n; j++)
            {
                b[k, j] = random.NextDouble();
            }
        }

        return new MatrixPair(a, b, n, m);
    }

    public override double[,] ExecuteTyped(MatrixPair input, MeasurementContext? context)
    {
        int n = input.N;
        int m = input.M;
        var a = input.A;
        var b = input.B;
        var c = new double[n, n];

        for (int i = 0; i < n; i++)
        {
            for (int k = 0; k < m; k++)
            {
                double aik = a[i, k];
                for (int j = 0; j < n; j++)
                {
                    c[i, j] += aik * b[k, j];
                }
            }
        }

        return c;
    }
}
