using FluentValidation;
using MediatR;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Tasks.Common;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Tasks.Commands.CreateTask;

/// <summary>Creates a new task.</summary>
public record CreateTaskCommand : IRequest<TaskDto>
{
    public string Title { get; init; } = string.Empty;

    public string? Description { get; init; }

    public TaskPriority Priority { get; init; } = TaskPriority.Medium;

    public DateTime? DueDateUtc { get; init; }
}

public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(command => command.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(command => command.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

        RuleFor(command => command.Priority)
            .IsInEnum().WithMessage("Priority must be Low, Medium or High.");
    }
}

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskDto>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTaskCommandHandler(ITaskRepository taskRepository, IUnitOfWork unitOfWork)
    {
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TaskDto> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var task = new TaskItem
        {
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            Priority = request.Priority,
            DueDateUtc = request.DueDateUtc,
            IsCompleted = false,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        _taskRepository.Add(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return TaskDto.FromEntity(task);
    }
}
