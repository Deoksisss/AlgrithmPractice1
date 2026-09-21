using AlgorithmPractice1.Core.Approximation;
using AlgorithmPractice1.Core.Models;
using AlgorithmPractice1.GUI.Models;
using AvaloniaCharts.Controls;
using AvaloniaCharts.Core;

namespace AlgorithmPractice1.GUI.Services;

public static class ChartBuilderService
{
    private static readonly string[] Palette =
    {
        "#0066CC", // Синий
        "#E65100", // Оранжевый
        "#2E7D32", // Зелёный
        "#8E24AA", // Фиолетовый
        "#00838F", // Бирюзовый
        "#D81B60", // Малиновый
        "#5D4037"  // Коричневый
    };

    public static ChartDisplayModel BuildSingleAlgorithmChart(SessionAlgorithm sa, string algorithmDisplayName)
    {
        bool isMatrix = sa.AlgorithmId == "MatrixMultiply";

        if (isMatrix)
        {
            return BuildMatrix3DChart(new[] { sa }, algorithmDisplayName);
        }

        return Build2DChart(sa, algorithmDisplayName);
    }

    private static ChartDisplayModel Build2DChart(SessionAlgorithm sa, string algorithmDisplayName)
    {
        var points = sa.Measurements;
        bool isSteps = sa.Measurements.Any(m => m.StepCount.HasValue);

        string yLabel = isSteps ? "Количество шагов (операций)" : "Время выполнения (мс)";

        var seriesList = new List<Chart2DSeries>();

        // 1. Эмпирическая серия
        var groupedPoints = points
            .GroupBy(p => p.N)
            .OrderBy(g => g.Key)
            .Select(g => new Point2D(
                g.Key,
                isSteps ? g.Average(p => p.StepCount.GetValueOrDefault(0)) : g.Average(p => p.ElapsedMilliseconds.GetValueOrDefault(0.0))))
            .ToList();

        if (groupedPoints.Count > 0)
        {
            seriesList.Add(new Chart2DSeries
            {
                Name = "Эмпирические данные",
                Points = groupedPoints,
                ColorHex = "#0055D4",
                LineStyle = LineStyle.Solid,
                LineThickness = 2.0
            });
        }

        // 2. Серия аппроксимации
        if (sa.Approximation != null && groupedPoints.Count > 1)
        {
            var approx = sa.Approximation;
            var funcType = ComplexityFunctionExtensions.FromKey(approx.FunctionType);

            int minN = (int)groupedPoints.First().X;
            int maxN = (int)groupedPoints.Last().X;
            int step = Math.Max(1, (maxN - minN) / 100);

            var approxPoints = new List<Point2D>();
            for (int n = minN; n <= maxN; n += step)
            {
                double fVal = LeastSquaresSolver.EvaluateBasisFunction(funcType, n);
                double yVal = approx.Coefficient * fVal;
                approxPoints.Add(new Point2D(n, yVal));
            }

            seriesList.Add(new Chart2DSeries
            {
                Name = approx.LegendLabel,
                Points = approxPoints,
                ColorHex = "#FF8F00", // Оранжево-янтарный
                LineStyle = LineStyle.Dashed,
                LineThickness = 2.5
            });
        }

        var chartParams = new Chart2DParams
        {
            Title = $"{algorithmDisplayName}",
            XLabel = "Размер входа N",
            YLabel = yLabel
        };

        return new ChartDisplayModel
        {
            Title = algorithmDisplayName,
            Subtitle = sa.Approximation != null
                ? $"МНК: {sa.Approximation.LegendLabel} | C = {sa.Approximation.Coefficient:E3}"
                : "Замеры выполнены",
            AlgorithmId = sa.AlgorithmId,
            Is3D = false,
            Series2D = seriesList,
            Params2D = chartParams
        };
    }

    public static ChartDisplayModel BuildMatrix3DChart(IReadOnlyList<SessionAlgorithm> matrixAlgorithms, string algorithmDisplayName)
    {
        var surfaceSeriesList = new List<Chart3DSeries>();

        for (int idx = 0; idx < matrixAlgorithms.Count; idx++)
        {
            var sa = matrixAlgorithms[idx];
            var measurements = sa.Measurements;
            if (measurements.Count == 0) continue;

            string colorHex = Palette[idx % Palette.Length];
            string seriesName = matrixAlgorithms.Count > 1
                ? $"Сессия #{sa.SessionId} (A: n×m × B: m×n)"
                : "Поверхность времени матричного умножения";

            // Группируем по N и M
            var distinctN = measurements.Select(m => (double)m.N).Distinct().OrderBy(v => v).ToArray();
            var distinctM = measurements.Select(m => (double)m.M.GetValueOrDefault(50)).Distinct().OrderBy(v => v).ToArray();

            SurfaceData surfaceData;

            // Если M фиксировано (одно значение), создаём полосу шириной вокруг M для построения 3D-сетки
            if (distinctM.Length == 1)
            {
                double fixedM = distinctM[0];
                double[] yVals = { Math.Max(1.0, fixedM - 10.0), fixedM, fixedM + 10.0 };
                double[,] zVals = new double[distinctN.Length, yVals.Length];

                var nMeanTime = measurements
                    .GroupBy(m => m.N)
                    .ToDictionary(g => g.Key, g => g.Average(p => p.ElapsedMilliseconds.GetValueOrDefault(0.0)));

                for (int i = 0; i < distinctN.Length; i++)
                {
                    int n = (int)distinctN[i];
                    double time = nMeanTime.TryGetValue(n, out double t) ? t : 0.0;
                    for (int j = 0; j < yVals.Length; j++)
                    {
                        zVals[i, j] = time;
                    }
                }

                surfaceData = new SurfaceData
                {
                    XValues = distinctN,
                    YValues = yVals,
                    Z = zVals
                };
            }
            else
            {
                // Полноценная сетка NxM
                double[,] zVals = new double[distinctN.Length, distinctM.Length];
                for (int i = 0; i < distinctN.Length; i++)
                {
                    int n = (int)distinctN[i];
                    for (int j = 0; j < distinctM.Length; j++)
                    {
                        int m = (int)distinctM[j];
                        var match = measurements.Where(p => p.N == n && p.M == m).ToList();
                        zVals[i, j] = match.Count > 0 ? match.Average(p => p.ElapsedMilliseconds.GetValueOrDefault(0.0)) : 0.0;
                    }
                }

                surfaceData = new SurfaceData
                {
                    XValues = distinctN,
                    YValues = distinctM,
                    Z = zVals
                };
            }

            surfaceSeriesList.Add(new Chart3DSeries
            {
                Name = seriesName,
                Data = surfaceData,
                ColorHex = colorHex,
                Style = SurfaceStyle.SolidWithWireframe,
                Opacity = 0.85
            });
        }

        var chartParams = new Chart3DParams
        {
            Title = $"{algorithmDisplayName} (3D)",
            XLabel = "Размер N",
            YLabel = "Размер M",
            ZLabel = "Время (мс)"
        };

        return new ChartDisplayModel
        {
            Title = algorithmDisplayName,
            Subtitle = "3D график зависимости времени от N и M (A: n×m, B: m×n)",
            AlgorithmId = "MatrixMultiply",
            Is3D = true,
            Series3D = surfaceSeriesList,
            Params3D = chartParams
        };
    }

    public static ChartDisplayModel BuildComparisonChart(
        string algorithmId,
        string algorithmDisplayName,
        IReadOnlyList<(BenchmarkSession Session, SessionAlgorithm SessionAlg)> algorithmsAcrossSessions)
    {
        if (algorithmId == "MatrixMultiply")
        {
            var saList = algorithmsAcrossSessions.Select(x => x.SessionAlg).ToList();
            return BuildMatrix3DChart(saList, algorithmDisplayName);
        }

        var seriesList = new List<Chart2DSeries>();
        bool isSteps = algorithmsAcrossSessions.Any(x => x.SessionAlg.Measurements.Any(m => m.StepCount.HasValue));
        string yLabel = isSteps ? "Количество шагов (операций)" : "Время выполнения (мс)";

        for (int i = 0; i < algorithmsAcrossSessions.Count; i++)
        {
            var (session, sa) = algorithmsAcrossSessions[i];
            string color = Palette[i % Palette.Length];

            // 1. Эмпирическая серия для этой сессии
            var grouped = sa.Measurements
                .GroupBy(m => m.N)
                .OrderBy(g => g.Key)
                .Select(g => new Point2D(
                    g.Key,
                    isSteps ? g.Average(p => p.StepCount.GetValueOrDefault(0)) : g.Average(p => p.ElapsedMilliseconds.GetValueOrDefault(0.0))))
                .ToList();

            if (grouped.Count > 0)
            {
                seriesList.Add(new Chart2DSeries
                {
                    Name = $"[{session.Label ?? $"Сессия #{session.Id}"}] Эмпирические",
                    Points = grouped,
                    ColorHex = color,
                    LineStyle = LineStyle.Solid,
                    LineThickness = 2.0
                });
            }

            // 2. Аппроксимация для этой сессии
            if (sa.Approximation != null && grouped.Count > 1)
            {
                var approx = sa.Approximation;
                var funcType = ComplexityFunctionExtensions.FromKey(approx.FunctionType);

                int minN = (int)grouped.First().X;
                int maxN = (int)grouped.Last().X;
                int step = Math.Max(1, (maxN - minN) / 80);

                var approxPoints = new List<Point2D>();
                for (int n = minN; n <= maxN; n += step)
                {
                    double fVal = LeastSquaresSolver.EvaluateBasisFunction(funcType, n);
                    approxPoints.Add(new Point2D(n, approx.Coefficient * fVal));
                }

                seriesList.Add(new Chart2DSeries
                {
                    Name = $"[{session.Label ?? $"Сессия #{session.Id}"}] Аппроксимация (MSE: {ApproximationResult.FormatMse(approx.Mse)})",
                    Points = approxPoints,
                    ColorHex = color,
                    LineStyle = LineStyle.Dashed,
                    LineThickness = 2.0
                });
            }
        }

        var chartParams = new Chart2DParams
        {
            Title = $"Сравнение: {algorithmDisplayName}",
            XLabel = "Размер входа N",
            YLabel = yLabel
        };

        return new ChartDisplayModel
        {
            Title = $"Сравнение: {algorithmDisplayName}",
            Subtitle = $"Сравнение по {algorithmsAcrossSessions.Count} сессиям",
            AlgorithmId = algorithmId,
            Is3D = false,
            Series2D = seriesList,
            Params2D = chartParams
        };
    }
}
