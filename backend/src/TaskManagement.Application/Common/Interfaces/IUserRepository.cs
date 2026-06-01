using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Common.Interfaces;

/// <summary>Persistence abstraction for <see cref="AppUser"/>.</summary>
public interface IUserRepository
{
    /// <summary>Returns the user with the given username, or null.</summary>
    Task<AppUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken);

    /// <summary>Returns true if any user exists (used by the seeder).</summary>
    Task<bool> AnyAsync(CancellationToken cancellationToken);

    void Add(AppUser user);
}
