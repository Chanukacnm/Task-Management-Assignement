namespace TaskManagement.Application.Common.Interfaces;

/// <summary>
/// Coordinates persisting changes made through the repositories as a single
/// atomic operation.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>Persists all pending changes and returns the number of affected rows.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
