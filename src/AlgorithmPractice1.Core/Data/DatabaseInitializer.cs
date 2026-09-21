using Dapper;

namespace AlgorithmPractice1.Core.Data;

/// <summary>
/// Инициализация структуры SQLite БД скриптом CREATE TABLE IF NOT EXISTS.
/// </summary>
public static class DatabaseInitializer
{
    private const string SchemaSql = @"
        CREATE TABLE IF NOT EXISTS Sessions (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            CreatedAt TEXT NOT NULL,
            Label TEXT NULL
        );

        CREATE TABLE IF NOT EXISTS SessionAlgorithms (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            SessionId INTEGER NOT NULL,
            AlgorithmId TEXT NOT NULL,
            ConfigJson TEXT NOT NULL,
            ConfigHash TEXT NOT NULL,
            FOREIGN KEY (SessionId) REFERENCES Sessions(Id) ON DELETE CASCADE
        );

        CREATE TABLE IF NOT EXISTS Measurements (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            SessionAlgorithmId INTEGER NOT NULL,
            N INTEGER NOT NULL,
            M INTEGER NULL,
            RunIndex INTEGER NOT NULL,
            ElapsedTicks INTEGER NULL,
            StepCount INTEGER NULL,
            FOREIGN KEY (SessionAlgorithmId) REFERENCES SessionAlgorithms(Id) ON DELETE CASCADE
        );

        CREATE TABLE IF NOT EXISTS ApproximationResults (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            SessionAlgorithmId INTEGER NOT NULL,
            FunctionType TEXT NOT NULL,
            Coefficient REAL NOT NULL,
            Mse REAL NOT NULL,
            FOREIGN KEY (SessionAlgorithmId) REFERENCES SessionAlgorithms(Id) ON DELETE CASCADE
        );

        CREATE INDEX IF NOT EXISTS idx_session_algorithms_hash ON SessionAlgorithms(AlgorithmId, ConfigHash);
        CREATE INDEX IF NOT EXISTS idx_measurements_sa_id ON Measurements(SessionAlgorithmId);
        CREATE INDEX IF NOT EXISTS idx_approx_sa_id ON ApproximationResults(SessionAlgorithmId);
    ";

    public static void Initialize(IDbConnectionFactory connectionFactory)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Execute(SchemaSql);
    }

    public static async Task InitializeAsync(IDbConnectionFactory connectionFactory)
    {
        using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteAsync(SchemaSql);
    }
}
