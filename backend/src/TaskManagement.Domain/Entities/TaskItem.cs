using TaskManagement.Domain.Enums;

namespace TaskManagement.Domain.Entities;

/// <summary>
/// A single task that a user can create, track and complete.
/// </summary>
public class TaskItem
{
    public int Id { get; set; }

    /// <summary>Short, required summary of the task.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Optional, longer description of what the task involves.</summary>
    public string? Description { get; set; }

    /// <summary>Whether the task has been marked as done.</summary>
    public bool IsCompleted { get; set; }

    /// <summary>Relative importance of the task.</summary>
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    /// <summary>Optional date by which the task should be completed (UTC).</summary>
    public DateTime? DueDateUtc { get; set; }

    /// <summary>When the task was created (UTC).</summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>When the task was last modified (UTC).</summary>
    public DateTime UpdatedAtUtc { get; set; }
}
