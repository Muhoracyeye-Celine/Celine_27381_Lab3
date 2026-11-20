using System.Security.Cryptography;

namespace SecureForms.Security;

public static class Passwords
{
    public static string Hash(string password)
    {
        if (password.Length < 8 || password.Length > 128)
            throw new InvalidOperationException("Use a password containing 8 to 128 characters.");
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100000, HashAlgorithmName.SHA256, 32);
        return $"100000.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public static bool Verify(string password, string stored)
    {
        try
        {
            string[] parts = stored.Split('.');
            byte[] expected = Convert.FromBase64String(parts[2]);
            byte[] actual = Rfc2898DeriveBytes.Pbkdf2(password, Convert.FromBase64String(parts[1]),
                int.Parse(parts[0]), HashAlgorithmName.SHA256, expected.Length);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (Exception e) when (e is FormatException or IndexOutOfRangeException or ArgumentException)
        {
            return false;
        }
    }
}
