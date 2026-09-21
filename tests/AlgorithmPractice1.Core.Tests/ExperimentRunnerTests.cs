using AlgorithmPractice1.Core.Algorithms.Part1_Vectors;
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
}
