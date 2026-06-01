using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Common.Models;

/// <summary>
/// Filtering and sorting options for a task list query. Passed from the
/// Application layer to the repository, which applies them at the data source.
/// </summary>
public record TaskListFilter
{
    /// <summary>"all" (default), "active" or "completed".</summary>
    public string? Status { get; init; }

    public TaskPriority? Priority { get; init; }

    /// <summary>Case-insensitive search over title and description.</summary>
    public string? Search { get; init; }

    /// <summary>"title", "priority", "duedate", "status" or "created" (default).</summary>
    public string? SortBy { get; init; }

    /// <summary>"asc" or "desc".</summary>
    public string? SortDir { get; init; }
}
