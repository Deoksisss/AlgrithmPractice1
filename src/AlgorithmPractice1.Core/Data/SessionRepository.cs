using AlgorithmPractice1.Core.Models;
using Dapper;

namespace AlgorithmPractice1.Core.Data;

public interface ISessionRepository
{
    Task<long> CreateSessionAsync(BenchmarkSession session);
    Task<IReadOnlyList<BenchmarkSession>> GetAllSessionsAsync();
    Task<BenchmarkSession?> GetSessionByIdAsync(long sessionId);
    Task DeleteSessionAsync(long sessionId);
    Task UpdateSessionLabelAsync(long sessionId, string? label);
}

public sealed class SessionRepository : ISessionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public SessionRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<long> CreateSessionAsync(BenchmarkSession session)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string insertSql = @"
            INSERT INTO Sessions (CreatedAt, Label)
            VALUES (@CreatedAt, @Label);
            SELECT last_insert_rowid();
        ";
        long id = await connection.ExecuteScalarAsync<long>(insertSql, new
        {
            session.CreatedAt,
            session.Label
        });
        return id;
    }

    public async Task<IReadOnlyList<BenchmarkSession>> GetAllSessionsAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sessionsSql = "SELECT * FROM Sessions ORDER BY Id DESC;";
        var sessions = (await connection.QueryAsync<BenchmarkSession>(sessionsSql)).ToList();

        if (sessions.Count == 0) return sessions;

        const string algorithmsSql = "SELECT * FROM SessionAlgorithms ORDER BY Id ASC;";
        var algorithms = (await connection.QueryAsync<SessionAlgorithm>(algorithmsSql)).ToList();
        var algorithmsBySessionId = algorithms.GroupBy(a => a.SessionId).ToDictionary(g => g.Key, g => g.ToList());

        foreach (var s in sessions)
        {
            if (algorithmsBySessionId.TryGetValue(s.Id, out var algList))
            {
                s.Algorithms.AddRange(algList);
            }
        }

        return sessions;
    }

    public async Task<BenchmarkSession?> GetSessionByIdAsync(long sessionId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sessionSql = "SELECT * FROM Sessions WHERE Id = @Id;";
        var session = await connection.QuerySingleOrDefaultAsync<BenchmarkSession>(sessionSql, new { Id = sessionId });
        if (session == null) return null;

        const string algorithmsSql = "SELECT * FROM SessionAlgorithms WHERE SessionId = @SessionId ORDER BY Id ASC;";
        var algorithms = (await connection.QueryAsync<SessionAlgorithm>(algorithmsSql, new { SessionId = sessionId })).ToList();

        const string measurementsSql = @"
            SELECT m.* FROM Measurements m
            JOIN SessionAlgorithms sa ON m.SessionAlgorithmId = sa.Id
            WHERE sa.SessionId = @SessionId
            ORDER BY m.N ASC, m.RunIndex ASC;
        ";
        var measurements = (await connection.QueryAsync<MeasurementPoint>(measurementsSql, new { SessionId = sessionId })).ToList();
        var measurementsByAlgId = measurements.GroupBy(m => m.SessionAlgorithmId).ToDictionary(g => g.Key, g => g.ToList());

        const string approxSql = @"
            SELECT a.* FROM ApproximationResults a
            JOIN SessionAlgorithms sa ON a.SessionAlgorithmId = sa.Id
            WHERE sa.SessionId = @SessionId;
        ";
        var approximations = (await connection.QueryAsync<ApproximationResult>(approxSql, new { SessionId = sessionId })).ToList();
        var approxByAlgId = approximations.ToDictionary(a => a.SessionAlgorithmId);

        foreach (var alg in algorithms)
        {
            if (measurementsByAlgId.TryGetValue(alg.Id, out var mList))
            {
                alg.Measurements.AddRange(mList);
            }

            if (approxByAlgId.TryGetValue(alg.Id, out var approx))
            {
                alg.Approximation = approx;
            }

            session.Algorithms.Add(alg);
        }

        return session;
    }

    public async Task DeleteSessionAsync(long sessionId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM Sessions WHERE Id = @Id;";
        await connection.ExecuteAsync(sql, new { Id = sessionId });
    }

    public async Task UpdateSessionLabelAsync(long sessionId, string? label)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "UPDATE Sessions SET Label = @Label WHERE Id = @Id;";
        await connection.ExecuteAsync(sql, new { Id = sessionId, Label = label });
    }
}
