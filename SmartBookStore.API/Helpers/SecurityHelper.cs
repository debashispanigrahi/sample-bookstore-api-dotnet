using System;

namespace SmartBookStore.API.Helpers;

public static class SecurityHelper
{
    public static bool VerifyPassword(string password, string salt, string hash)
    {
        ArgumentNullException.ThrowIfNull(password);
        ArgumentNullException.ThrowIfNull(salt);
        ArgumentNullException.ThrowIfNull(hash);

        var computedHash = HashPassword(password, salt);
        return computedHash == hash;
    }

    public static string HashPassword(string password, string salt)
    {
        using var pbkdf2 = new System.Security.Cryptography.Rfc2898DeriveBytes(
            password,
            Convert.FromBase64String(salt),
            10000,
            System.Security.Cryptography.HashAlgorithmName.SHA256);
        return Convert.ToBase64String(pbkdf2.GetBytes(32));
    }

    public static string GenerateSalt()
    {
        var bytes = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}
