using TaskManagement.Application.Common.Interfaces;

namespace TaskManagement.Infrastructure.Persistence;

/// <summary>
/// Unit-of-work implementation backed by the EF Core <see cref="ApplicationDbContext"/>.
/// Repositories and this unit of work share the same scoped context instance, so
/// changes made through the repositories are persisted by a single SaveChanges call.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
