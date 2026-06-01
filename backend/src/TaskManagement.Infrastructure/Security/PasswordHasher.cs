using System.Security.Cryptography;
using TaskManagement.Application.Common.Interfaces;

namespace TaskManagement.Infrastructure.Security;

/// <summary>
/// Hashes passwords using PBKDF2 (RFC 2898) with a per-password random salt and
/// SHA-256. The stored value has the form "iterations;saltBase64;hashBase64" so
/// the parameters travel with the hash and verification is self-describing.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;             // 128-bit salt
    private const int KeySize = 32;              // 256-bit derived key
    private const int Iterations = 600_000;      // OWASP guidance for PBKDF2-HMAC-SHA256
    private const int MinimumIterations = 10_000; // reject implausibly weak stored hashes
    private const char Delimiter = ';';
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);

        return string.Join(
            Delimiter,
            Iterations,
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));
    }

    public bool Verify(string password, string passwordHash)
    {
        var segments = passwordHash.Split(Delimiter);
        if (segments.Length != 3)
        {
            return false;
        }

        if (!int.TryParse(segments[0], out var iterations) || iterations < MinimumIterations)
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(segments[1]);
            var hash = Convert.FromBase64String(segments[2]);

            // Reject malformed/tampered stored hashes. Without this, an empty hash
            // segment would derive a zero-length key and FixedTimeEquals would return
            // true for any password.
            if (hash.Length != KeySize)
            {
                return false;
            }

            var inputHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, Algorithm, hash.Length);

            // Constant-time comparison to avoid timing attacks.
            return CryptographicOperations.FixedTimeEquals(hash, inputHash);
        }
        catch (Exception ex) when (ex is FormatException or ArgumentOutOfRangeException)
        {
            return false;
        }
    }
}
