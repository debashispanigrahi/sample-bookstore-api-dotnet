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
    private readonly string _connectionString;

    public UserRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE Username = @Username AND IsActive = 1",
            new { Username = username });
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE Email = @Email AND IsActive = 1",
            new { Email = email });
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE Id = @Id AND IsActive = 1",
            new { Id = id });
    }

    public async Task<User> CreateUserAsync(RegisterRequest request)
    {
        var salt = GenerateSalt();
        var passwordHash = HashPassword(request.Password, salt);

        using var connection = new SqlConnection(_connectionString);
        
        var userId = await connection.QuerySingleAsync<int>(@"
            INSERT INTO Users (Username, Email, PasswordHash, Salt, Role, CreatedAt) 
            OUTPUT INSERTED.Id
            VALUES (@Username, @Email, @PasswordHash, @Salt, @Role, @CreatedAt);",
            new
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                Salt = salt,
                Role = request.Role,
                CreatedAt = DateTime.UtcNow
            });

        return await GetByIdAsync(userId) ?? throw new InvalidOperationException("Failed to create user");
    }

    public async Task UpdateLastLoginAsync(int userId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(
            "UPDATE Users SET LastLoginAt = @LastLoginAt WHERE Id = @Id",
            new { Id = userId, LastLoginAt = DateTime.UtcNow });
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