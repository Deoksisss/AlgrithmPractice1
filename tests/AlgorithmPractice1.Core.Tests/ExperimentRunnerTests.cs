using AlgorithmPractice1.Core.Algorithms.Part1_Vectors;
using AlgorithmPractice1.Core.Algorithms.Part3_Individual;
using AlgorithmPractice1.Core.Algorithms.Part4_Exponentiation;
using AlgorithmPractice1.Core.Data;
using AlgorithmPractice1.Core.Execution;
using AlgorithmPractice1.Core.Models;
using Xunit;

namespace AlgorithmPractice1.Core.Tests;

public class ExperimentRunnerTests
{
    [Fact]
    public async Task RunBatchAsync_ExecutesBatch_CachesResults_AndUsesCacheOnSecondRun()
    {
        var factory = SqliteConnectionFactory.CreateInMemory();
        using var keepAlive = factory.CreateConnection();
        await DatabaseInitializer.InitializeAsync(factory);

        var sessionRepo = new SessionRepository(factory);
        var measurementRepo = new MeasurementRepository(factory);
        var approxRepo = new ApproximationRepository(factory);
        var runner = new ExperimentRunner(sessionRepo, measurementRepo, approxRepo);

        var alg1 = new ConstantFunctionAlgorithm();
        var config1 = new ExperimentConfig { NMax = 100, NStep = 50, RunsPerN = 2, ForceRecalculate = false };

        var alg2 = new IterativeExponentiationAlgorithm();
        var config2 = new ExperimentConfig { NMax = 100, NStep = 50, RunsPerN = 1, ForceRecalculate = false };

        var requests = new List<ExperimentRequest>
        {
            new(alg1, config1),
            new(alg2, config2)
        };

        var progressUpdates = new List<SessionProgressUpdate>();
        var progress = new Progress<SessionProgressUpdate>(u => progressUpdates.Add(u));

        // 1. Первый запуск (холодный)
        var session1 = await runner.RunBatchAsync(requests, "Session 1", progress);

        Assert.Equal(2, session1.Algorithms.Count);
        Assert.True(session1.Algorithms[0].Measurements.Count > 0);
        Assert.True(session1.Algorithms[1].Measurements.Count > 0);
        Assert.NotNull(session1.Algorithms[0].Approximation);
        Assert.NotNull(session1.Algorithms[1].Approximation);

        // 2. Второй запуск той же конфигурации (должен использовать кэш)
        SessionProgressUpdate? lastUpdate = null;
        var progress2 = new Progress<SessionProgressUpdate>(u => lastUpdate = u);

        var session2 = await runner.RunBatchAsync(requests, "Session 2", progress2);

        Assert.Equal(2, session2.Algorithms.Count);
        Assert.True(session2.Algorithms[0].Measurements.Count > 0);

        // 3. Запуск с ForceRecalculate = true
        var forceRequests = new List<ExperimentRequest>
        {
            new(alg1, config1 with { ForceRecalculate = true })
        };
        var session3 = await runner.RunBatchAsync(forceRequests, "Session 3");
        Assert.Single(session3.Algorithms);
    }

    [Fact]
    public async Task RunBatchAsync_ExecutesMatrixGrid_CoversAllPairsOfNandM()
    {
        var factory = SqliteConnectionFactory.CreateInMemory();
        using var keepAlive = factory.CreateConnection();
        await DatabaseInitializer.InitializeAsync(factory);

        var sessionRepo = new SessionRepository(factory);
        var measurementRepo = new MeasurementRepository(factory);
        var approxRepo = new ApproximationRepository(factory);
        var runner = new ExperimentRunner(sessionRepo, measurementRepo, approxRepo);

        var matrixAlg = new AlgorithmPractice1.Core.Algorithms.Part2_Matrices.MatrixMultiplicationAlgorithm();
        // N in {20, 40}, M in {20, 40} -> 4 pairs of (N, M), 1 run each
        var config = new ExperimentConfig
        {
            NMax = 40,
            NStep = 20,
            M = 40,
            MStep = 20,
            RunsPerN = 1
        };

        var requests = new List<ExperimentRequest> { new(matrixAlg, config) };
        var session = await runner.RunBatchAsync(requests, "Matrix Grid Session");

        Assert.Single(session.Algorithms);
        var algResult = session.Algorithms[0];
        Assert.Equal(4, algResult.Measurements.Count);

        var pairs = algResult.Measurements.Select(m => (m.N, m.M.GetValueOrDefault())).OrderBy(p => p.N).ThenBy(p => p.Item2).ToList();
        Assert.Contains((20, 20), pairs);
        Assert.Contains((20, 40), pairs);
        Assert.Contains((40, 20), pairs);
        Assert.Contains((40, 40), pairs);
    }

    [Fact]
    public async Task RunBatchAsync_ReportsProgress_WithDetailsFor1DAndMillerRabin()
    {
        var factory = SqliteConnectionFactory.CreateInMemory();
        using var keepAlive = factory.CreateConnection();
        await DatabaseInitializer.InitializeAsync(factory);

        var sessionRepo = new SessionRepository(factory);
        var measurementRepo = new MeasurementRepository(factory);
        var approxRepo = new ApproximationRepository(factory);
        var runner = new ExperimentRunner(sessionRepo, measurementRepo, approxRepo);

        var vectorAlg = new ConstantFunctionAlgorithm();
        var vectorConfig = new ExperimentConfig { NMax = 20, NStep = 10, RunsPerN = 1, ForceRecalculate = true };

        var mrAlg = new MillerRabinAlgorithm();
        var mrConfig = new ExperimentConfig { NMax = 32, NStep = 16, K = 15, RunsPerN = 1, ForceRecalculate = true };

        var requests = new List<ExperimentRequest>
        {
            new(vectorAlg, vectorConfig),
            new(mrAlg, mrConfig)
        };

        var progressUpdates = new List<SessionProgressUpdate>();
        var progress = new Progress<SessionProgressUpdate>(u =>
        {
            // Snapshot current items
            progressUpdates.Add(new SessionProgressUpdate
            {
                CompletedAlgorithms = u.CompletedAlgorithms,
                TotalAlgorithms = u.TotalAlgorithms,
                CurrentRunningAlgorithmId = u.CurrentRunningAlgorithmId,
                Items = u.Items.Select(it => new AlgorithmProgressItem
                {
                    AlgorithmId = it.AlgorithmId,
                    AlgorithmName = it.AlgorithmName,
                    Status = it.Status,
                    CurrentN = it.CurrentN,
                    TotalNCount = it.TotalNCount,
                    Details = it.Details
                }).ToList()
            });
        });

        await runner.RunBatchAsync(requests, "Progress Details Test", progress);

        // Проверяем, что для одномерного алгоритма были обновления со строкой N = ... (X/Y)
        var vectorRunningDetails = progressUpdates
            .SelectMany(u => u.Items)
            .Where(it => it.AlgorithmId == vectorAlg.Id && it.Details != null && it.Details.StartsWith("N ="))
            .Select(it => it.Details)
            .ToList();

        Assert.NotEmpty(vectorRunningDetails);
        Assert.Contains(vectorRunningDetails, d => d!.Contains("N = 10 (1/2)") || d!.Contains("N = 20 (2/2)"));

        // Проверяем, что для Миллера-Рабина детали содержали K = 15
        var mrRunningDetails = progressUpdates
            .SelectMany(u => u.Items)
            .Where(it => it.AlgorithmId == mrAlg.Id && it.Details != null && it.Details.StartsWith("N ="))
            .Select(it => it.Details)
            .ToList();

        Assert.NotEmpty(mrRunningDetails);
        Assert.Contains(mrRunningDetails, d => d!.Contains("K = 15"));
    }
}
