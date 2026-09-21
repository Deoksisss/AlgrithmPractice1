namespace AlgorithmPractice1.Core.Data;

/// <summary>
/// Определение пути к корню решения и файлу БД SQLite.
/// </summary>
public static class SolutionPathResolver
{
    public const string DefaultDbFileName = "algorithm_practice.db";

    public static string FindSolutionDirectory()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "AlgorithmPractice1.sln")) ||
                File.Exists(Path.Combine(dir.FullName, "AlgorithmPractice1.slnx")))
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }

        // Запасной путь для сценариев отладки / scratch
        string fallback = @"C:\Users\Deoksisss\.gemini\antigravity\scratch\AlgorithmPractice1";
        if (Directory.Exists(fallback))
        {
            return fallback;
        }

        return AppContext.BaseDirectory;
    }

    public static string GetDatabasePath()
    {
        return Path.Combine(FindSolutionDirectory(), DefaultDbFileName);
    }

    public static SqliteConnectionFactory CreateDefaultFactory()
    {
        string path = GetDatabasePath();
        return SqliteConnectionFactory.FromFilePath(path);
    }
}
