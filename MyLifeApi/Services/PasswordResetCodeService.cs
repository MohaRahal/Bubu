using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using MyLifeApi.Options;

namespace MyLifeApi.Services;

public sealed class PasswordResetCodeService(IOptions<PasswordResetOptions> options)
{
    private readonly byte[] _hashKey = Encoding.UTF8.GetBytes(options.Value.HashKey);

    public static string GenerateCode() =>
        RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

    public static string GenerateSalt() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));

    public static string GenerateValidationToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    public string Hash(string salt, string code)
    {
        using var hmac = new HMACSHA256(_hashKey);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes($"{salt}:{code}"));
        return Convert.ToHexString(hash);
    }

    public bool Verify(string salt, string expectedHash, string code)
    {
        var actualHash = Convert.FromHexString(Hash(salt, code));
        var storedHash = Convert.FromHexString(expectedHash);
        return CryptographicOperations.FixedTimeEquals(actualHash, storedHash);
    }
}
