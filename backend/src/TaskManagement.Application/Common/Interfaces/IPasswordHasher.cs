namespace TaskManagement.Application.Common.Interfaces;

/// <summary>
/// Hashes and verifies passwords. Implemented in the Infrastructure layer using
/// a salted, iterated key-derivation function (PBKDF2).
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Creates a salted hash for the given plain-text password.</summary>
    string Hash(string password);

    /// <summary>Verifies a plain-text password against a previously stored hash.</summary>
    bool Verify(string password, string passwordHash);
}
