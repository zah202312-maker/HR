using System;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace HR_Mangment2;

/// <summary>
/// Each test project owns one PostgreSQL database, because test projects can run in parallel.
/// The name always carries "_Test_", so the tests can never touch the development database.
/// </summary>
public static class TestDatabase
{
    public const string ConnectionVariable = "HR_TEST_CONNECTION";

    public static string ConnectionStringFor(string projectSuffix)
    {
        var baseConnection = Environment.GetEnvironmentVariable(ConnectionVariable) ?? ReadHostConnection();
        var databaseName = "HR_Mangment2_Test_" + projectSuffix;

        if (!Regex.IsMatch(baseConnection, "Database=[^;]*", RegexOptions.IgnoreCase))
        {
            throw new InvalidOperationException("The test connection string has no Database value.");
        }

        return Regex.Replace(baseConnection, "Database=[^;]*", "Database=" + databaseName, RegexOptions.IgnoreCase);
    }

    public static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "HR_Mangment2.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }

    private static string ReadHostConnection()
    {
        var secretsPath = Path.Combine(FindRepositoryRoot(), "src", "HR_Mangment2.HttpApi.Host", "appsettings.secrets.json");
        if (!File.Exists(secretsPath))
        {
            throw new InvalidOperationException($"Set {ConnectionVariable}, or create {secretsPath}.");
        }

        using var document = JsonDocument.Parse(File.ReadAllText(secretsPath));
        return document.RootElement.GetProperty("ConnectionStrings").GetProperty("Default").GetString()!;
    }
}
