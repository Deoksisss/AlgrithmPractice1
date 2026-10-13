using AlgorithmPractice1.Core.Approximation;
using AlgorithmPractice1.Core.Models;
using AlgorithmPractice1.Core.Registry;
using AlgorithmPractice1.GUI.Models;
using AvaloniaCharts.Controls;
using AvaloniaCharts.Core;

namespace AlgorithmPractice1.GUI.Services;

public static class ChartBuilderService
{
    private static readonly string[] Palette =
    {
        "#0066CC", // Синий
        "#D81B60", // Малиновый
        "#2E7D32", // Зелёный
        "#E65100", // Оранжевый
        "#8E24AA", // Фиолетовый
        "#00838F", // Бирюзовый
        "#F57F17", // Жёлто-янтарный
        "#5D4037", // Коричневый
        "#1B5E20", // Тёмно-зелёный
        "#C2185B", // Рубиновый
        "#0288D1", // Голубой
        "#6A1B9A"  // Тёмно-фиолетовый
    };

    private static readonly string[] ApproxPalette =
    {
        "#FF8F00", // Янтарный
        "#FF3D00", // Красно-оранжевый
        "#00E676", // Светло-зелёный
        "#E040FB", // Светло-фиолетовый
        "#00E5FF", // Светло-бирюзовый
        "#FF4081", // Светло-малиновый
        "#FFAB00"  // Золотистый
    };

    /// <summary>
    /// Строит единый общий график для любого набора выбранных тестов (одной или нескольких сессий).
    /// </summary>
    public static ChartDisplayModel? BuildUnifiedChart(
        IReadOnlyList<(BenchmarkSession Session, SessionAlgorithm SessionAlg)> selectedTests)
    {
        if (selectedTests == null || selectedTests.Count == 0)
        {
            return null;
        }

        bool is3D = selectedTests.All(x => x.SessionAlg.AlgorithmId == "MatrixMultiply");

        return is3D ? BuildUnified3DChart(selectedTests) : BuildUnified2DChart(selectedTests);
    }

    private static ChartDisplayModel BuildUnified2DChart(
        IReadOnlyList<(BenchmarkSession Session, SessionAlgorithm SessionAlg)> selectedTests)
    {
        var seriesList = new List<Chart2DSeries>();
        bool isSteps = selectedTests.Any(x => x.SessionAlg.Measurements.Any(m => m.StepCount.HasValue));
        string yLabel = isSteps ? "Количество шагов (операций)" : "Время выполнения (мс)";

        bool multipleSessions = selectedTests.Select(x => x.Session.Id).Distinct().Count() > 1;
        bool hasDuplicateAlgs = selectedTests.GroupBy(x => x.SessionAlg.AlgorithmId).Any(g => g.Count() > 1);
        bool showSessionTag = multipleSessions || hasDuplicateAlgs;

        var seenCount = new Dictionary<(long SessionId, string AlgId), int>();

        for (int i = 0; i < selectedTests.Count; i++)
        {
            var (session, sa) = selectedTests[i];
            string color = Palette[i % Palette.Length];

            var algMeta = AlgorithmRegistry.Instance.FindById(sa.AlgorithmId);
            string algDisplayName = algMeta?.DisplayName ?? sa.AlgorithmId;
            string sessionTag = string.IsNullOrWhiteSpace(session.Label) ? $"Сессия #{session.Id}" : session.Label;

            var key = (session.Id, sa.AlgorithmId);
            int currentCount = seenCount.GetValueOrDefault(key, 0) + 1;
            seenCount[key] = currentCount;

            string duplicateSuffix = currentCount > 1 ? $" #{currentCount}" : string.Empty;

            string baseLabel = showSessionTag
                ? $"{algDisplayName} [{sessionTag}]{duplicateSuffix}"
                : $"{algDisplayName}{duplicateSuffix}";

            // 1. Эмпирическая серия
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
                    Name = baseLabel,
                    Points = grouped,
                    ColorHex = color,
                    LineStyle = LineStyle.Solid,
                    LineThickness = 2.0
                });
            }

            // 2. Аппроксимация
            var approx = sa.Approximation;
            if (approx == null && grouped.Count > 1 && algMeta != null)
            {
                var fitPoints = sa.Measurements
                    .Select(m => ((double)m.N, isSteps ? (double)m.StepCount.GetValueOrDefault(0) : m.ElapsedMilliseconds.GetValueOrDefault(0.0)))
                    .ToList();
                approx = LeastSquaresSolver.Fit(fitPoints, algMeta.TheoreticalComplexity, sa.Id);
            }

            if (approx != null && grouped.Count > 1)
            {
                var funcType = ComplexityFunctionExtensions.FromKey(approx.FunctionType);
                int minN = (int)grouped.First().X;
                int maxN = (int)grouped.Last().X;
                int step = Math.Max(1, (maxN - minN) / 100);

                var approxPoints = new List<Point2D>();
                for (int n = minN; n <= maxN; n += step)
                {
                    double fVal = LeastSquaresSolver.EvaluateBasisFunction(funcType, n);
                    approxPoints.Add(new Point2D(n, approx.Coefficient * fVal));
                }

                string approxLabel = $"{baseLabel} — {funcType.ToDisplayName()} (MSE: {ApproximationResult.FormatMse(approx.Mse)})";

                seriesList.Add(new Chart2DSeries
                {
                    Name = approxLabel,
                    Points = approxPoints,
                    ColorHex = color,
                    LineStyle = LineStyle.Dashed,
                    LineThickness = 2.0
                });
            }
        }

        string title;
        if (selectedTests.Count == 1)
        {
            var first = selectedTests[0];
            var meta = AlgorithmRegistry.Instance.FindById(first.SessionAlg.AlgorithmId);
            title = meta?.DisplayName ?? first.SessionAlg.AlgorithmId;
        }
        else
        {
            var distinctAlgs = selectedTests.Select(x => x.SessionAlg.AlgorithmId).Distinct().ToList();
            if (distinctAlgs.Count == 1)
            {
                var meta = AlgorithmRegistry.Instance.FindById(distinctAlgs[0]);
                title = $"Сравнение: {meta?.DisplayName ?? distinctAlgs[0]} ({selectedTests.Count} тестов)";
            }
            else
            {
                title = $"Сравнение алгоритмов ({selectedTests.Count} тестов)";
            }
        }

        int sessionsCount = selectedTests.Select(x => x.Session.Id).Distinct().Count();
        string subtitle = selectedTests.Count == 1
            ? $"Сессия #{selectedTests[0].Session.Id} ({selectedTests[0].Session.Label ?? "без метки"})"
            : $"Выбрано {selectedTests.Count} тестов из {sessionsCount} сессий";

        var chartParams = new Chart2DParams
        {
            Title = title,
            XLabel = "Размер входа N",
            YLabel = yLabel
        };

        return new ChartDisplayModel
        {
            Title = title,
            Subtitle = subtitle,
            AlgorithmId = selectedTests.Count == 1 ? selectedTests[0].SessionAlg.AlgorithmId : "Multiple",
            Is3D = false,
            Series2D = seriesList,
            Params2D = chartParams
        };
    }

    private static ChartDisplayModel BuildUnified3DChart(
        IReadOnlyList<(BenchmarkSession Session, SessionAlgorithm SessionAlg)> selectedTests)
    {
        var surfaceSeriesList = new List<Chart3DSeries>();

        for (int idx = 0; idx < selectedTests.Count; idx++)
        {
            var (session, sa) = selectedTests[idx];
            var measurements = sa.Measurements;
            if (measurements.Count == 0) continue;

            string colorHex = Palette[idx % Palette.Length];
            string approxColorHex = ApproxPalette[idx % ApproxPalette.Length];
            string sessionTag = string.IsNullOrWhiteSpace(session.Label) ? $"Сессия #{session.Id}" : session.Label;

            string empiricalSeriesName = selectedTests.Count > 1
                ? $"[{sessionTag}] Замеры"
                : "Эмпирические замеры (время, мс)";

            var distinctN = measurements.Select(m => (double)m.N).Distinct().OrderBy(v => v).ToArray();
            var distinctM = measurements.Select(m => (double)m.M.GetValueOrDefault(50)).Distinct().OrderBy(v => v).ToArray();

            SurfaceData surfaceData;

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
                Name = empiricalSeriesName,
                Data = surfaceData,
                ColorHex = colorHex,
                Style = SurfaceStyle.SolidWithWireframe,
                Opacity = 0.75
            });

            // Аппроксимация O(n²·m)
            var approx = sa.Approximation;
            if (approx == null && measurements.Count > 1)
            {
                var pts = measurements
                    .Select(m => ((double)m.N, (double)m.M.GetValueOrDefault(50), m.ElapsedMilliseconds.GetValueOrDefault(0.0)))
                    .ToList();
                approx = LeastSquaresSolver.FitMatrix3D(pts, sa.Id);
            }

            if (approx != null)
            {
                var xVals = surfaceData.XValues;
                var yVals = surfaceData.YValues;
                double[,] zApprox = new double[xVals.Length, yVals.Length];

                for (int i = 0; i < xVals.Length; i++)
                {
                    double n = xVals[i];
                    for (int j = 0; j < yVals.Length; j++)
                    {
                        double m = yVals[j];
                        zApprox[i, j] = approx.Coefficient * (n * n * m);
                    }
                }

                string approxSeriesName = selectedTests.Count > 1
                    ? $"[{sessionTag}] {approx.LegendLabel}"
                    : approx.LegendLabel;

                surfaceSeriesList.Add(new Chart3DSeries
                {
                    Name = approxSeriesName,
                    Data = new SurfaceData
                    {
                        XValues = xVals,
                        YValues = yVals,
                        Z = zApprox
                    },
                    ColorHex = approxColorHex,
                    Style = SurfaceStyle.Wireframe,
                    Opacity = 0.95
                });
            }
        }

        int sessionCount = selectedTests.Select(x => x.Session.Id).Distinct().Count();
        string title = selectedTests.Count > 1
            ? $"Сравнение: Умножение матриц (3D, {selectedTests.Count} тестов)"
            : "Умножение матриц (3D)";

        string subtitle = selectedTests.Count > 1
            ? $"Сравнение 3D поверхностей по {selectedTests.Count} тестам из {sessionCount} сессий"
            : "3D график зависимости времени от N и M (A: n×m, B: m×n)";

        var chartParams = new Chart3DParams
        {
            Title = title,
            XLabel = "Размер N",
            YLabel = "Размер M",
            ZLabel = "Время (мс)"
        };

        return new ChartDisplayModel
        {
            Title = title,
            Subtitle = subtitle,
            AlgorithmId = "MatrixMultiply",
            Is3D = true,
            Series3D = surfaceSeriesList,
            Params3D = chartParams
        };
    }
}
