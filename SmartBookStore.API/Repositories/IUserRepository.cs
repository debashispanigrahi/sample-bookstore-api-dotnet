using Dapper;
using Microsoft.Data.SqlClient;
using SmartBookStore.API.Models;

namespace SmartBookStore.API.Repositories;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int id);
    Task<User> CreateUserAsync(RegisterRequest request);
    Task UpdateLastLoginAsync(int userId);
}

public class UserRepository : IUserRepository
{
    public UserRepository()
    {
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        using var connection = DbConnectionFactory.Create();
        return await connection.QueryFirstOrDefaultAsync<User>(
            "usp_GetUserByUsername",
            new { Username = username },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        using var connection = DbConnectionFactory.Create();
        return await connection.QueryFirstOrDefaultAsync<User>(
            "usp_GetUserByEmail",
            new { Email = email },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        using var connection = DbConnectionFactory.Create();
        return await connection.QueryFirstOrDefaultAsync<User>(
            "usp_GetUserById",
            new { Id = id },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<User> CreateUserAsync(RegisterRequest request)
    {
        var salt = GenerateSalt();
        var passwordHash = HashPassword(request.Password, salt);

        using var connection = DbConnectionFactory.Create();
        var parameters = new DynamicParameters();
        parameters.Add("@Username", request.Username);
        parameters.Add("@Email", request.Email);
        parameters.Add("@PasswordHash", passwordHash);
        parameters.Add("@Salt", salt);
        parameters.Add("@Role", request.Role);
        parameters.Add("@CreatedAt", DateTime.UtcNow);
        parameters.Add("@NewId", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync("usp_CreateUser", parameters, commandType: System.Data.CommandType.StoredProcedure);
        var newId = parameters.Get<int>("@NewId");

        return await GetByIdAsync(newId) ?? throw new InvalidOperationException("Failed to create user");
    }

    public async Task UpdateLastLoginAsync(int userId)
    {
        using var connection = DbConnectionFactory.Create();
        await connection.ExecuteAsync(
            "usp_UpdateLastLogin",
            new { Id = userId, LastLoginAt = DateTime.UtcNow },
            commandType: System.Data.CommandType.StoredProcedure);
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

    public static bool VerifyPassword(string password, string salt, string hash)
    {
        var computedHash = HashPassword(password, salt);
        return computedHash == hash;
    }
}