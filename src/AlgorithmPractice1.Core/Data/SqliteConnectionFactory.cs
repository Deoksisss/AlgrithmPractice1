using System.Data;
using Microsoft.Data.Sqlite;

namespace AlgorithmPractice1.Core.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}

public sealed class SqliteConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqliteConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public static SqliteConnectionFactory FromFilePath(string dbFilePath)
    {
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = dbFilePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            ForeignKeys = true
        };
        return new SqliteConnectionFactory(builder.ConnectionString);
    }

    public static SqliteConnectionFactory CreateInMemory(string? dbName = null)
    {
        string name = dbName ?? ("TestDb_" + Guid.NewGuid().ToString("N"));
        return new SqliteConnectionFactory($"Data Source={name};Mode=Memory;Cache=Shared;Foreign Keys=True");
    }

    public IDbConnection CreateConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }
}
