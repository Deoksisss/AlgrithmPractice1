using AlgorithmPractice1.Core.Data;
using AlgorithmPractice1.Core.Models;
using Xunit;

namespace AlgorithmPractice1.Core.Tests;

public class DatabaseRepositoryTests
{
    [Fact]
    public async Task SessionAndMeasurementRepositories_WorkEndToEndWithCache()
    {
        // Для in-memory базы сохраняем открытое соединение на время теста
        var factory = SqliteConnectionFactory.CreateInMemory();
        using var keepAliveConnection = factory.CreateConnection();

        await DatabaseInitializer.InitializeAsync(factory);

        var sessionRepo = new SessionRepository(factory);
        var measurementRepo = new MeasurementRepository(factory);
        var approxRepo = new ApproximationRepository(factory);

        // 1. Создаём сессию
        var session = new BenchmarkSession
        {
            CreatedAt = DateTime.UtcNow.ToString("o"),
            Label = "Тестовая сессия"
        };
        long sessionId = await sessionRepo.CreateSessionAsync(session);
        Assert.True(sessionId > 0);

        // 2. Добавляем алгоритм сессии
        var config = new ExperimentConfig { NMax = 500, NStep = 100, RunsPerN = 3 };
        string hash = config.ComputeConfigHash();
        long saId = await measurementRepo.AddSessionAlgorithmAsync(sessionId, "BubbleSort", config.ToJson(), hash);
        Assert.True(saId > 0);

        // 3. Сохраняем замеры
        var measurements = new List<MeasurementPoint>
        {
            new() { SessionAlgorithmId = saId, N = 100, RunIndex = 1, ElapsedTicks = 1200 },
            new() { SessionAlgorithmId = saId, N = 100, RunIndex = 2, ElapsedTicks = 1150 },
            new() { SessionAlgorithmId = saId, N = 200, RunIndex = 1, ElapsedTicks = 4500 }
        };
        await measurementRepo.SaveMeasurementsAsync(saId, measurements);

        // 4. Сохраняем аппроксимацию
        var approx = new ApproximationResult
        {
            SessionAlgorithmId = saId,
            FunctionType = "n2",
            Coefficient = 0.0012,
            Mse = 0.00004
        };
        await approxRepo.SaveApproximationAsync(approx);

        // 5. Проверяем извлечение сессии
        var loadedSession = await sessionRepo.GetSessionByIdAsync(sessionId);
        Assert.NotNull(loadedSession);
        Assert.Single(loadedSession.Algorithms);
        Assert.Equal(3, loadedSession.Algorithms[0].Measurements.Count);
        Assert.NotNull(loadedSession.Algorithms[0].Approximation);
        Assert.Equal("n2", loadedSession.Algorithms[0].Approximation!.FunctionType);

        // 6. Проверяем кэш
        var cached = await measurementRepo.GetCachedMeasurementsAsync("BubbleSort", hash);
        Assert.NotNull(cached);
        Assert.Equal(3, cached.Count);

        // Другой хэш не должен найтись
        var notCached = await measurementRepo.GetCachedMeasurementsAsync("BubbleSort", "unknown_hash");
        Assert.Null(notCached);
    }

    [Fact]
    public void SolutionPathResolver_CreatesRealDatabaseFileAtSolutionRoot()
    {
        string dbPath = SolutionPathResolver.GetDatabasePath();
        Assert.NotNull(dbPath);
        Assert.EndsWith(SolutionPathResolver.DefaultDbFileName, dbPath);

        var factory = SolutionPathResolver.CreateDefaultFactory();
        DatabaseInitializer.Initialize(factory);

        Assert.True(File.Exists(dbPath));
    }
}
