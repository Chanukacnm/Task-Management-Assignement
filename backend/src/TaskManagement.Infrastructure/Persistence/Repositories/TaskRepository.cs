using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Common.Models;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Persistence.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly ApplicationDbContext _context;

    public TaskRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<TaskItem>> GetTasksAsync(TaskListFilter filter, CancellationToken cancellationToken)
    {
        IQueryable<TaskItem> query = _context.Tasks.AsNoTracking();

        // --- Filtering -----------------------------------------------------
        query = filter.Status?.Trim().ToLowerInvariant() switch
        {
            "active" => query.Where(task => !task.IsCompleted),
            "completed" => query.Where(task => task.IsCompleted),
            _ => query
        };

        if (filter.Priority.HasValue)
        {
            query = query.Where(task => task.Priority == filter.Priority.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            // Lower-case both sides so the search is case-insensitive regardless
            // of the database collation (translated to SQL LOWER(...) LIKE).
            var term = filter.Search.Trim().ToLower();
            query = query.Where(task =>
                task.Title.ToLower().Contains(term) ||
                (task.Description != null && task.Description.ToLower().Contains(term)));
        }

        // --- Sorting (with a stable Id tie-breaker for deterministic order) -
        var sortBy = string.IsNullOrWhiteSpace(filter.SortBy)
            ? "created"
            : filter.SortBy.Trim().ToLowerInvariant();

        // Default to newest-first when sorting by creation date and no direction is given.
        var descending = string.IsNullOrWhiteSpace(filter.SortDir)
            ? sortBy == "created"
            : string.Equals(filter.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

        query = (sortBy, descending) switch
        {
            ("title", false) => query.OrderBy(t => t.Title).ThenBy(t => t.Id),
            ("title", true) => query.OrderByDescending(t => t.Title).ThenByDescending(t => t.Id),
            ("priority", false) => query.OrderBy(t => t.Priority).ThenBy(t => t.Id),
            ("priority", true) => query.OrderByDescending(t => t.Priority).ThenByDescending(t => t.Id),
            ("duedate", false) => query.OrderBy(t => t.DueDateUtc).ThenBy(t => t.Id),
            ("duedate", true) => query.OrderByDescending(t => t.DueDateUtc).ThenByDescending(t => t.Id),
            ("status", false) => query.OrderBy(t => t.IsCompleted).ThenBy(t => t.Id),
            ("status", true) => query.OrderByDescending(t => t.IsCompleted).ThenByDescending(t => t.Id),
            (_, false) => query.OrderBy(t => t.CreatedAtUtc).ThenBy(t => t.Id),
            (_, true) => query.OrderByDescending(t => t.CreatedAtUtc).ThenByDescending(t => t.Id)
        };

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<TaskItem?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        // Tracked: callers may mutate the returned entity and persist via the unit of work.
        return await _context.Tasks.FirstOrDefaultAsync(task => task.Id == id, cancellationToken);
    }

    public void Add(TaskItem task) => _context.Tasks.Add(task);

    public void Remove(TaskItem task) => _context.Tasks.Remove(task);
}
