using System.Diagnostics;
using AlgorithmPractice1.Core.Abstractions;
using AlgorithmPractice1.Core.Approximation;
using AlgorithmPractice1.Core.Data;
using AlgorithmPractice1.Core.Models;
using AlgorithmPractice1.Core.Registry;

namespace AlgorithmPractice1.Core.Execution;

public record ExperimentRequest(IAlgorithm Algorithm, ExperimentConfig Config);

/// <summary>
/// Движок выполнения пакетных экспериментов, управления кэшем и сохранения результатов в БД.
/// </summary>
public sealed class ExperimentRunner
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IMeasurementRepository _measurementRepository;
    private readonly IApproximationRepository _approximationRepository;
    private readonly AlgorithmRegistry _registry;

    public ExperimentRunner(
        ISessionRepository sessionRepository,
        IMeasurementRepository measurementRepository,
        IApproximationRepository approximationRepository,
        AlgorithmRegistry? registry = null)
    {
        _sessionRepository = sessionRepository;
        _measurementRepository = measurementRepository;
        _approximationRepository = approximationRepository;
        _registry = registry ?? AlgorithmRegistry.Instance;
    }

    /// <summary>
    /// Выполняет пакетный запуск списка выбранных алгоритмов с их конфигурациями.
    /// </summary>
    public async Task<BenchmarkSession> RunBatchAsync(
        IReadOnlyList<ExperimentRequest> requests,
        string? sessionLabel = null,
        IProgress<SessionProgressUpdate>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (requests == null || requests.Count == 0)
        {
            throw new ArgumentException("Список алгоритмов для запуска не может быть пустым.", nameof(requests));
        }

        // 1. Создаём сессию в БД
        var session = new BenchmarkSession
        {
            CreatedAt = DateTime.UtcNow.ToString("o"),
            Label = string.IsNullOrWhiteSpace(sessionLabel)
                ? $"Сессия #{DateTime.Now:yyyy-MM-dd HH:mm:ss}"
                : sessionLabel.Trim()
        };

        long sessionId = await _sessionRepository.CreateSessionAsync(session);
        session = session with { Id = sessionId };

        // Инициализируем элементы прогресса
        var progressItems = requests.Select(r =>
        {
            int nStep = Math.Max(1, r.Config.NStep);
            int nMax = Math.Max(nStep, r.Config.NMax);
            int nPoints = (nMax - nStep) / nStep + 1;

            int totalGridPoints = nPoints;
            if (r.Algorithm.Category == AlgorithmCategory.Matrices)
            {
                int mStep = Math.Max(1, r.Config.MStep.GetValueOrDefault(nStep));
                int mMax = Math.Max(mStep, r.Config.M.GetValueOrDefault(nMax));
                int mPoints = (mMax - mStep) / mStep + 1;
                totalGridPoints = nPoints * mPoints;
            }

            return new AlgorithmProgressItem
            {
                AlgorithmId = r.Algorithm.Id,
                AlgorithmName = r.Algorithm.DisplayName,
                Status = AlgorithmExecutionStatus.Queued,
                CurrentN = 0,
                TotalNCount = totalGridPoints
            };
        }).ToList();

        int completedCount = 0;

        void ReportProgress(string? currentRunningId)
        {
            progress?.Report(new SessionProgressUpdate
            {
                CompletedAlgorithms = completedCount,
                TotalAlgorithms = requests.Count,
                Items = progressItems.ToList(),
                CurrentRunningAlgorithmId = currentRunningId
            });
        }

        ReportProgress(null);

        // 2. Последовательно прогоняем каждый алгоритм
        for (int i = 0; i < requests.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var req = requests[i];
            var alg = req.Algorithm;
            var config = req.Config;
            var item = progressItems[i];

            string configHash = config.ComputeConfigHash();
            string configJson = config.ToJson();

            // Создаем запись алгоритма для текущей сессии
            long sessionAlgorithmId = await _measurementRepository.AddSessionAlgorithmAsync(
                sessionId, alg.Id, configJson, configHash);

            var sessionAlgorithm = new SessionAlgorithm
            {
                Id = sessionAlgorithmId,
                SessionId = sessionId,
                AlgorithmId = alg.Id,
                ConfigJson = configJson,
                ConfigHash = configHash
            };

            // Проверка кэша
            IReadOnlyList<MeasurementPoint>? cachedPoints = null;
            if (!config.ForceRecalculate)
            {
                cachedPoints = await _measurementRepository.GetCachedMeasurementsAsync(alg.Id, configHash);
            }

            if (cachedPoints != null && cachedPoints.Count > 0)
            {
                // Кэш сработал!
                item.Status = AlgorithmExecutionStatus.CompletedCached;
                item.Details = $"Взято из кэша ({cachedPoints.Count} замеров)";

                // Копируем закэшированные замеры в текущую сессию
                var pointsForSession = cachedPoints.Select(p => p with
                {
                    Id = 0,
                    SessionAlgorithmId = sessionAlgorithmId
                }).ToList();

                await _measurementRepository.SaveMeasurementsAsync(sessionAlgorithmId, pointsForSession);
                sessionAlgorithm.Measurements.AddRange(pointsForSession);

                // Считаем или достаем аппроксимацию
                var approx = ComputeApproximation(alg, sessionAlgorithmId, pointsForSession);
                await _approximationRepository.SaveApproximationAsync(approx);
                sessionAlgorithm.Approximation = approx;

                completedCount++;
                ReportProgress(alg.Id);
                session.Algorithms.Add(sessionAlgorithm);
                continue;
            }

            // Иначе — реальный расчет
            item.Status = AlgorithmExecutionStatus.Running;
            ReportProgress(alg.Id);

            var calculatedPoints = new List<MeasurementPoint>();
            int step = Math.Max(1, config.NStep);
            int nMax = Math.Max(step, config.NMax);
            int runs = Math.Max(1, config.RunsPerN);

            if (alg.Category == AlgorithmCategory.Matrices)
            {
                // Двумерная сетка (N x M) для матричного умножения
                int mStep = Math.Max(1, config.MStep.GetValueOrDefault(step));
                int mMax = Math.Max(mStep, config.M.GetValueOrDefault(nMax));
                int totalGridPoints = ((nMax - step) / step + 1) * ((mMax - mStep) / mStep + 1);
                int completedPoints = 0;

                // Прогрев на небольшой матрице
                try
                {
                    var warmupInput = alg.GenerateInput(Math.Min(nMax, 50), config with { M = Math.Min(mMax, 50) });
                    alg.Execute(warmupInput, null);
                }
                catch { }

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                for (int n = step; n <= nMax; n += step)
                {
                    for (int m = mStep; m <= mMax; m += mStep)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        completedPoints++;
                        item.CurrentN = completedPoints;
                        item.Details = $"N = {n}, M = {m} ({completedPoints}/{totalGridPoints})";
                        ReportProgress(alg.Id);

                        var runConfig = config with { M = m };

                        for (int run = 1; run <= runs; run++)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            object input = alg.GenerateInput(n, runConfig);
                            long start = Stopwatch.GetTimestamp();
                            alg.Execute(input, null);
                            long elapsedTicks = Stopwatch.GetTimestamp() - start;

                            calculatedPoints.Add(new MeasurementPoint
                            {
                                SessionAlgorithmId = sessionAlgorithmId,
                                N = n,
                                M = m,
                                RunIndex = run,
                                ElapsedTicks = elapsedTicks,
                                StepCount = null
                            });
                        }
                    }
                }
            }
            else
            {
                // Одномерные алгоритмы по N
                for (int n = step; n <= nMax; n += step)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    item.CurrentN = n;
                    ReportProgress(alg.Id);

                    // Прогрев перед серией запусков для режима Time
                    if (alg.MeasurementType == MeasurementType.Time)
                    {
                        try
                        {
                            var warmupInput = alg.GenerateInput(Math.Min(n, 100), config);
                            alg.Execute(warmupInput, null);
                        }
                        catch { /* Игнорируем ошибки прогрева */ }

                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        GC.Collect();
                    }

                    for (int run = 1; run <= runs; run++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        object input = alg.GenerateInput(n, config);

                        if (alg.MeasurementType == MeasurementType.Time)
                        {
                            long start = Stopwatch.GetTimestamp();
                            alg.Execute(input, null);
                            long elapsedTicks = Stopwatch.GetTimestamp() - start;

                            calculatedPoints.Add(new MeasurementPoint
                            {
                                SessionAlgorithmId = sessionAlgorithmId,
                                N = n,
                                M = config.M,
                                RunIndex = run,
                                ElapsedTicks = elapsedTicks,
                                StepCount = null
                            });
                        }
                        else
                        {
                            var context = new MeasurementContext();
                            alg.Execute(input, context);

                            calculatedPoints.Add(new MeasurementPoint
                            {
                                SessionAlgorithmId = sessionAlgorithmId,
                                N = n,
                                M = config.M,
                                RunIndex = run,
                                ElapsedTicks = null,
                                StepCount = context.StepCount
                            });
                        }
                    }
                }
            }

            // Сохраняем рассчитанные точки
            await _measurementRepository.SaveMeasurementsAsync(sessionAlgorithmId, calculatedPoints);
            sessionAlgorithm.Measurements.AddRange(calculatedPoints);

            // Аппроксимация
            var calcApprox = ComputeApproximation(alg, sessionAlgorithmId, calculatedPoints);
            await _approximationRepository.SaveApproximationAsync(calcApprox);
            sessionAlgorithm.Approximation = calcApprox;

            item.Status = AlgorithmExecutionStatus.CompletedCalculated;
            item.Details = $"Посчитано ({calculatedPoints.Count} замеров)";
            completedCount++;
            ReportProgress(alg.Id);

            session.Algorithms.Add(sessionAlgorithm);
        }

        ReportProgress(null);
        return session;
    }

    private static ApproximationResult ComputeApproximation(
        IAlgorithm algorithm,
        long sessionAlgorithmId,
        IReadOnlyList<MeasurementPoint> points)
    {
        if (algorithm.Category == AlgorithmCategory.Matrices || algorithm.TheoreticalComplexity == ComplexityFunctionType.Matrix3D)
        {
            var matrixPoints = points
                .GroupBy(p => (p.N, M: p.M.GetValueOrDefault(p.N)))
                .Select(g => (
                    N: (double)g.Key.N,
                    M: (double)g.Key.M,
                    EmpiricalValue: algorithm.MeasurementType == MeasurementType.Time
                        ? g.Average(p => p.ElapsedMilliseconds.GetValueOrDefault(0.0))
                        : g.Average(p => (double)p.StepCount.GetValueOrDefault(0))
                ))
                .ToList();

            return LeastSquaresSolver.FitMatrix3D(matrixPoints, sessionAlgorithmId);
        }

        // Усредняем эмпирические значения для каждого N
        var groupedPoints = points
            .GroupBy(p => p.N)
            .Select(g =>
            {
                double n = g.Key;
                double avgValue = algorithm.MeasurementType == MeasurementType.Time
                    ? g.Average(p => p.ElapsedMilliseconds.GetValueOrDefault(0.0))
                    : g.Average(p => (double)p.StepCount.GetValueOrDefault(0));
                return (N: n, EmpiricalValue: avgValue);
            })
            .OrderBy(p => p.N)
            .ToList();

        return LeastSquaresSolver.Fit(groupedPoints, algorithm.TheoreticalComplexity, sessionAlgorithmId);
    }
}
