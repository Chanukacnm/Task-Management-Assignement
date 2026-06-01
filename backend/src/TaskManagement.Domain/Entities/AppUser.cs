namespace TaskManagement.Domain.Entities;

/// <summary>
/// An application user whose credentials are used for HTTP Basic authentication.
/// Passwords are never stored in plain text — only a salted PBKDF2 hash is kept.
/// </summary>
public class AppUser
{
    public int Id { get; set; }

    /// <summary>Unique login name.</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>Friendly name shown in the UI.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Salted PBKDF2 password hash in the form "iterations;salt;hash".</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>When the user was created (UTC).</summary>
    public DateTime CreatedAtUtc { get; set; }
}
