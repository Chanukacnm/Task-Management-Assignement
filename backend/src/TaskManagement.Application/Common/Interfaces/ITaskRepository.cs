using TaskManagement.Application.Common.Models;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Common.Interfaces;

/// <summary>
/// Persistence abstraction for <see cref="TaskItem"/>. Implemented in the
/// Infrastructure layer so the Application layer stays persistence-ignorant.
/// </summary>
public interface ITaskRepository
{
    /// <summary>Returns tasks matching the supplied filter, already sorted.</summary>
    Task<IReadOnlyList<TaskItem>> GetTasksAsync(TaskListFilter filter, CancellationToken cancellationToken);

    /// <summary>Returns a single task by id, or null if it does not exist.</summary>
    Task<TaskItem?> GetByIdAsync(int id, CancellationToken cancellationToken);

    void Add(TaskItem task);

    void Remove(TaskItem task);
}
