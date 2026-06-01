namespace TaskManagement.Application.Authentication.Common;

/// <summary>Public representation of an authenticated user (never includes the password hash).</summary>
public class UserDto
{
    public int Id { get; init; }

    public string Username { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;
}
