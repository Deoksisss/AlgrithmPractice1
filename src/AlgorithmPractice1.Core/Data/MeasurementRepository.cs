using AlgorithmPractice1.Core.Models;
using Dapper;

namespace AlgorithmPractice1.Core.Data;

public interface IMeasurementRepository
{
    Task<long> AddSessionAlgorithmAsync(long sessionId, string algorithmId, string configJson, string configHash);
    Task SaveMeasurementsAsync(long sessionAlgorithmId, IEnumerable<MeasurementPoint> measurements);
    Task<IReadOnlyList<MeasurementPoint>> GetMeasurementsAsync(long sessionAlgorithmId);
    Task<IReadOnlyList<MeasurementPoint>?> GetCachedMeasurementsAsync(string algorithmId, string configHash);
}

public sealed class MeasurementRepository : IMeasurementRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public MeasurementRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<long> AddSessionAlgorithmAsync(long sessionId, string algorithmId, string configJson, string configHash)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string insertSql = @"
            INSERT INTO SessionAlgorithms (SessionId, AlgorithmId, ConfigJson, ConfigHash)
            VALUES (@SessionId, @AlgorithmId, @ConfigJson, @ConfigHash);
            SELECT last_insert_rowid();
        ";
        return await connection.ExecuteScalarAsync<long>(insertSql, new
        {
            SessionId = sessionId,
            AlgorithmId = algorithmId,
            ConfigJson = configJson,
            ConfigHash = configHash
        });
    }

    public async Task SaveMeasurementsAsync(long sessionAlgorithmId, IEnumerable<MeasurementPoint> measurements)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string insertSql = @"
            INSERT INTO Measurements (SessionAlgorithmId, N, M, RunIndex, ElapsedTicks, StepCount)
            VALUES (@SessionAlgorithmId, @N, @M, @RunIndex, @ElapsedTicks, @StepCount);
        ";

        var pointsToInsert = measurements.Select(m => new
        {
            SessionAlgorithmId = sessionAlgorithmId,
            m.N,
            m.M,
            m.RunIndex,
            m.ElapsedTicks,
            m.StepCount
        });

        await connection.ExecuteAsync(insertSql, pointsToInsert);
    }

    public async Task<IReadOnlyList<MeasurementPoint>> GetMeasurementsAsync(long sessionAlgorithmId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM Measurements
            WHERE SessionAlgorithmId = @SessionAlgorithmId
            ORDER BY N ASC, RunIndex ASC;
        ";
        var results = await connection.QueryAsync<MeasurementPoint>(sql, new { SessionAlgorithmId = sessionAlgorithmId });
        return results.ToList();
    }

    public async Task<IReadOnlyList<MeasurementPoint>?> GetCachedMeasurementsAsync(string algorithmId, string configHash)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT m.* FROM Measurements m
            JOIN SessionAlgorithms sa ON m.SessionAlgorithmId = sa.Id
            WHERE sa.AlgorithmId = @AlgorithmId AND sa.ConfigHash = @ConfigHash
            ORDER BY sa.Id DESC, m.N ASC, m.RunIndex ASC;
        ";

        var points = (await connection.QueryAsync<MeasurementPoint>(sql, new
        {
            AlgorithmId = algorithmId,
            ConfigHash = configHash
        })).ToList();

        if (points.Count == 0) return null;

        // Если в истории несколько сессий с тем же хэшем, берём точки из самой свежей сессии
        long latestSaId = points[0].SessionAlgorithmId;
        return points.Where(p => p.SessionAlgorithmId == latestSaId).ToList();
    }
}
