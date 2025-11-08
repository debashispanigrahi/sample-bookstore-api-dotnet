using System;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace SmartBookStore.API.Repositories;

public static class DbConnectionFactory
{
    private static readonly string? _defaultConnectionString;

    static DbConnectionFactory()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables();

        var configuration = builder.Build();
        _defaultConnectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public static SqlConnection Create(string? connectionString = null)
    {
        var cs = string.IsNullOrWhiteSpace(connectionString) ? _defaultConnectionString : connectionString;

        if (string.IsNullOrWhiteSpace(cs))
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        return new SqlConnection(cs);
    }
}
