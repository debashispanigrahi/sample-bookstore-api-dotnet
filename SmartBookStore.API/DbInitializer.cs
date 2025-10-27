using System;
using Dapper;
using Microsoft.Data.SqlClient;

namespace SmartBookStore.API;

public static class DbInitializer
{
    public static void InitializeDatabase(string connectionString)
    {
        // Ensure the target database exists by connecting to the server 'master' database first
        var builder = new SqlConnectionStringBuilder(connectionString);
        var databaseName = builder.InitialCatalog;
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            // Fallback database name if none specified
            databaseName = "SmartBookStoreDB";
            builder.InitialCatalog = databaseName;
            connectionString = builder.ConnectionString;
        }

        var masterBuilder = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = "master"
        };

        using (var masterConnection = new SqlConnection(masterBuilder.ConnectionString))
        {
            masterConnection.Open();

            var createDbCmd = $@"IF DB_ID(N'{databaseName}') IS NULL
                                BEGIN
                                    CREATE DATABASE [{databaseName}];
                                END";

            masterConnection.Execute(createDbCmd);
        }

        using var connection = new SqlConnection(connectionString);
        connection.Open();

        // Create Books table
        var booksTableCmd = @"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Books]') AND type in (N'U'))
                            BEGIN
                                CREATE TABLE [Books] (
                                    [Id] INT PRIMARY KEY IDENTITY(1,1),
                                    [Title] NVARCHAR(500) NOT NULL,
                                    [Author] NVARCHAR(300) NOT NULL,
                                    [Genre] NVARCHAR(100),
                                    [Price] DECIMAL(10,2)
                                );
                            END";
        connection.Execute(booksTableCmd);

        // Create Users table
        var usersTableCmd = @"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
                            BEGIN
                                CREATE TABLE [Users] (
                                    [Id] INT PRIMARY KEY IDENTITY(1,1),
                                    [Username] NVARCHAR(100) NOT NULL UNIQUE,
                                    [Email] NVARCHAR(255) NOT NULL UNIQUE,
                                    [PasswordHash] NVARCHAR(500) NOT NULL,
                                    [Salt] NVARCHAR(500) NOT NULL,
                                    [Role] NVARCHAR(50) NOT NULL DEFAULT 'User',
                                    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                                    [LastLoginAt] DATETIME2 NULL,
                                    [IsActive] BIT NOT NULL DEFAULT 1
                                );
                            END";
        connection.Execute(usersTableCmd);

        // Create default admin user if not exists
        var adminExists = connection.QueryFirstOrDefault<int>(
            "SELECT COUNT(*) FROM Users WHERE Username = @Username", 
            new { Username = "admin" });

        if (adminExists == 0)
        {
            var salt = GenerateSalt();
            var passwordHash = HashPassword("admin123", salt);
            
            connection.Execute(@"INSERT INTO Users (Username, Email, PasswordHash, Salt, Role, CreatedAt) 
                               VALUES (@Username, @Email, @PasswordHash, @Salt, @Role, @CreatedAt)",
                new
                {
                    Username = "admin",
                    Email = "admin@smartbookstore.com",
                    PasswordHash = passwordHash,
                    Salt = salt,
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow
                });
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