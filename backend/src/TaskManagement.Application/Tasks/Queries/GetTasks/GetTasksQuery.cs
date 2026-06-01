using FluentValidation;
using MediatR;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Common.Models;
using TaskManagement.Application.Tasks.Common;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Tasks.Queries.GetTasks;

/// <summary>
/// Returns tasks, optionally filtered by completion status, priority and a free-text
/// search term, and sorted by a chosen field and direction.
/// </summary>
public record GetTasksQuery : IRequest<IReadOnlyList<TaskDto>>
{
    /// <summary>Filter by status: "all" (default), "active" or "completed".</summary>
    public string? Status { get; init; }

    /// <summary>Optional filter by priority.</summary>
    public TaskPriority? Priority { get; init; }

    /// <summary>Optional case-insensitive search over title and description.</summary>
    public string? Search { get; init; }

    /// <summary>Field to sort by: "title", "priority", "duedate", "status" or "created" (default).</summary>
    public string? SortBy { get; init; }

    /// <summary>Sort direction: "asc" or "desc".</summary>
    public string? SortDir { get; init; }
}

public class GetTasksQueryValidator : AbstractValidator<GetTasksQuery>
{
    private static readonly string[] AllowedStatuses = { "all", "active", "completed" };
    private static readonly string[] AllowedSortFields = { "created", "title", "priority", "duedate", "status" };
    private static readonly string[] AllowedSortDirections = { "asc", "desc" };

    public GetTasksQueryValidator()
    {
        When(query => !string.IsNullOrWhiteSpace(query.Status), () =>
            RuleFor(query => query.Status!)
                .Must(value => AllowedStatuses.Contains(value.ToLowerInvariant()))
                .WithMessage("Status must be 'all', 'active' or 'completed'."));

        When(query => query.Priority.HasValue, () =>
            RuleFor(query => query.Priority!.Value)
                .IsInEnum()
                .WithMessage("Priority must be Low, Medium or High."));

        When(query => !string.IsNullOrWhiteSpace(query.SortBy), () =>
            RuleFor(query => query.SortBy!)
                .Must(value => AllowedSortFields.Contains(value.ToLowerInvariant()))
                .WithMessage("SortBy must be one of: created, title, priority, duedate, status."));

        When(query => !string.IsNullOrWhiteSpace(query.SortDir), () =>
            RuleFor(query => query.SortDir!)
                .Must(value => AllowedSortDirections.Contains(value.ToLowerInvariant()))
                .WithMessage("SortDir must be 'asc' or 'desc'."));
    }
}

public class GetTasksQueryHandler : IRequestHandler<GetTasksQuery, IReadOnlyList<TaskDto>>
{
    private readonly ITaskRepository _taskRepository;

    public GetTasksQueryHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<IReadOnlyList<TaskDto>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        var filter = new TaskListFilter
        {
            Status = request.Status,
            Priority = request.Priority,
            Search = request.Search,
            SortBy = request.SortBy,
            SortDir = request.SortDir
        };

        var tasks = await _taskRepository.GetTasksAsync(filter, cancellationToken);

        return tasks.Select(TaskDto.FromEntity).ToList();
    }
}
