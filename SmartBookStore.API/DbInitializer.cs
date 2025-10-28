using System;
using System.IO;
using System.Text.RegularExpressions;
using Dapper;
using Microsoft.Data.SqlClient;

namespace SmartBookStore.API;

public static class DbInitializer
{
    private const string DefaultAdminPassword = "admin123";

    public static void InitializeDatabase(string connectionString)
    {
        // Generate admin credentials for the sql script
        var adminSalt = GenerateSalt();
        var adminPasswordHash = HashPassword(DefaultAdminPassword, adminSalt);

        // Ensure database exists and is accessible
        EnsureDatabaseExists(connectionString, out var databaseName);

        // Execute all database scripts
        using var connection = new SqlConnection(connectionString);
        connection.Open();

        ExecuteDatabaseScripts(connection, adminSalt, adminPasswordHash);
    }

    private static void EnsureDatabaseExists(string connectionString, out string databaseName)
    {
        Console.WriteLine("\n=== Database Connection Setup ===");
        var builder = new SqlConnectionStringBuilder(connectionString);
        databaseName = builder.InitialCatalog;

        if (string.IsNullOrWhiteSpace(databaseName))
        {
            databaseName = "SmartBookStoreDB";
            builder.InitialCatalog = databaseName;
            connectionString = builder.ConnectionString;
            Console.WriteLine($"✓ Using default database name: {databaseName}");
        }
        else
        {
            Console.WriteLine($"✓ Using provided database name: {databaseName}");
        }

        var masterBuilder = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = "master"
        };

        try
        {
            Console.WriteLine("▶ Connecting to SQL Server...");
            using var masterConnection = new SqlConnection(masterBuilder.ConnectionString);
            masterConnection.Open();
            Console.WriteLine("✓ Connected to SQL Server successfully");

            var createDbCmd = $@"IF DB_ID(N'{databaseName}') IS NULL
                                BEGIN
                                    CREATE DATABASE [{databaseName}];
                                    PRINT 'Database {databaseName} created successfully';
                                END
                                ELSE
                                BEGIN
                                    PRINT 'Database {databaseName} already exists';
                                END";

            masterConnection.Execute(createDbCmd);
            Console.WriteLine($"✓ Database '{databaseName}' is ready");
            Console.WriteLine("===========================\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Database connection error: {ex.Message}");
            Console.WriteLine("===========================\n");
            throw;
        }
    }

    private static void ExecuteDatabaseScripts(SqlConnection connection, string adminSalt, string adminPasswordHash)
    {
        var dbScriptsPath = Path.Combine(AppContext.BaseDirectory, "DbScripts");
        Console.WriteLine("\n=== Database Initialization Started ===");
        
        if (!Directory.Exists(dbScriptsPath))
        {
            Console.WriteLine($"❌ Error: DbScripts directory not found at: {dbScriptsPath}");
            throw new DirectoryNotFoundException($"DbScripts directory not found at: {dbScriptsPath}");
        }
        Console.WriteLine($"✓ Found scripts directory: {dbScriptsPath}");

        var scriptFiles = Directory.GetFiles(dbScriptsPath, "*.sql")
                                 .OrderBy(f => Path.GetFileName(f))
                                 .ToList();

        if (scriptFiles.Count == 0)
        {
            Console.WriteLine("❌ Error: No SQL scripts found in directory");
            throw new FileNotFoundException($"No SQL scripts found in: {dbScriptsPath}");
        }
        Console.WriteLine($"✓ Found {scriptFiles.Count} SQL scripts to execute\n");

        var successCount = 0;
        var failureCount = 0;

        foreach (var scriptFile in scriptFiles)
        {
            try 
            {
                ExecuteSqlScript(connection, scriptFile, adminSalt, adminPasswordHash);
                successCount++;
            }
            catch (Exception)
            {
                failureCount++;
            }
        }

        Console.WriteLine("\n=== Database Initialization Summary ===");
        Console.WriteLine($"Total Scripts: {scriptFiles.Count}");
        Console.WriteLine($"Successful: {successCount} ✓");
        Console.WriteLine($"Failed: {failureCount} ❌");
        Console.WriteLine("=====================================\n");

        if (failureCount > 0)
        {
            throw new Exception($"Database initialization completed with {failureCount} failed scripts");
        }
    }

    private static void ExecuteSqlScript(SqlConnection connection, string scriptFile, string adminSalt, string adminPasswordHash)
    {
        var fileName = Path.GetFileName(scriptFile);
        Console.WriteLine($"\n▶ Executing: {fileName}");
        Console.WriteLine("----------------------------------------");

        try
        {
            var scriptContent = File.ReadAllText(scriptFile);
            Console.WriteLine("✓ Script file read successfully");
            
            // Replace variables in script
            scriptContent = scriptContent.Replace("$(DefaultAdminSalt)", adminSalt);
            scriptContent = scriptContent.Replace("$(DefaultAdminPasswordHash)", adminPasswordHash);
            Console.WriteLine("✓ Variables replaced in script");

            // Split script on GO statements while preserving the original formatting
            var batches = Regex.Split(scriptContent, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)
                              .Where(batch => !string.IsNullOrWhiteSpace(batch))
                              .ToList();
            
            Console.WriteLine($"✓ Found {batches.Count} batch(es) to execute");

            var batchNumber = 1;
            foreach (var batch in batches)
            {
                if (!string.IsNullOrWhiteSpace(batch))
                {
                    try
                    {
                        connection.Execute(batch);
                        Console.WriteLine($"  ✓ Batch {batchNumber} executed successfully");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  ❌ Batch {batchNumber} failed: {ex.Message}");
                        throw;
                    }
                    batchNumber++;
                }
            }

            Console.WriteLine($"✅ Successfully completed: {fileName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in {fileName}: {ex.Message}");
            Console.WriteLine("----------------------------------------");
            throw new Exception($"Failed to execute script {fileName}", ex);
        }
    }

    private static string GenerateSalt()
    {
        var bytes = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static string HashPassword(string password, string salt)
    {
        using var pbkdf2 = new System.Security.Cryptography.Rfc2898DeriveBytes(
            password, 
            Convert.FromBase64String(salt), 
            10000, 
            System.Security.Cryptography.HashAlgorithmName.SHA256);
        return Convert.ToBase64String(pbkdf2.GetBytes(32));
    }
}