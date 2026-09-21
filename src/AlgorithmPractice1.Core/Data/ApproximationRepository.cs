using AlgorithmPractice1.Core.Models;
using Dapper;

namespace AlgorithmPractice1.Core.Data;

public interface IApproximationRepository
{
    Task SaveApproximationAsync(ApproximationResult approximation);
    Task<ApproximationResult?> GetApproximationAsync(long sessionAlgorithmId);
}

public sealed class ApproximationRepository : IApproximationRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ApproximationRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task SaveApproximationAsync(ApproximationResult approximation)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string insertSql = @"
            INSERT INTO ApproximationResults (SessionAlgorithmId, FunctionType, Coefficient, Mse)
            VALUES (@SessionAlgorithmId, @FunctionType, @Coefficient, @Mse);
        ";
        await connection.ExecuteAsync(insertSql, approximation);
    }

    public async Task<ApproximationResult?> GetApproximationAsync(long sessionAlgorithmId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM ApproximationResults WHERE SessionAlgorithmId = @SessionAlgorithmId LIMIT 1;";
        return await connection.QuerySingleOrDefaultAsync<ApproximationResult>(sql, new { SessionAlgorithmId = sessionAlgorithmId });
    }
}
