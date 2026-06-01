using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Tasks.Common;

/// <summary>
/// Data transfer object returned by the API for a task. Keeps the domain entity
/// decoupled from the public contract.
/// </summary>
public class TaskDto
{
    public int Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string? Description { get; init; }

    public bool IsCompleted { get; init; }

    public TaskPriority Priority { get; init; }

    public DateTime? DueDateUtc { get; init; }

    public DateTime CreatedAtUtc { get; init; }

    public DateTime UpdatedAtUtc { get; init; }

    public static TaskDto FromEntity(TaskItem task) => new()
    {
        Id = task.Id,
        Title = task.Title,
        Description = task.Description,
        IsCompleted = task.IsCompleted,
        Priority = task.Priority,
        DueDateUtc = task.DueDateUtc,
        CreatedAtUtc = task.CreatedAtUtc,
        UpdatedAtUtc = task.UpdatedAtUtc
    };
}
