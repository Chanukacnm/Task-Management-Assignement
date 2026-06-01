using FluentValidation;
using MediatR;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Tasks.Common;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Tasks.Commands.UpdateTask;

/// <summary>Updates an existing task's details.</summary>
public record UpdateTaskCommand : IRequest<TaskDto>
{
    public int Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string? Description { get; init; }

    public bool IsCompleted { get; init; }

    public TaskPriority Priority { get; init; } = TaskPriority.Medium;

    public DateTime? DueDateUtc { get; init; }
}

public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskCommandValidator()
    {
        RuleFor(command => command.Id)
            .GreaterThan(0).WithMessage("A valid task id is required.");

        RuleFor(command => command.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(command => command.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

        RuleFor(command => command.Priority)
            .IsInEnum().WithMessage("Priority must be Low, Medium or High.");
    }
}

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, TaskDto>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTaskCommandHandler(ITaskRepository taskRepository, IUnitOfWork unitOfWork)
    {
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TaskDto> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(request.Id, cancellationToken);

        if (task is null)
        {
            throw new NotFoundException(nameof(Domain.Entities.TaskItem), request.Id);
        }

        task.Title = request.Title.Trim();
        task.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        task.IsCompleted = request.IsCompleted;
        task.Priority = request.Priority;
        task.DueDateUtc = request.DueDateUtc;
        task.UpdatedAtUtc = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return TaskDto.FromEntity(task);
    }
}
