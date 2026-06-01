using MediatR;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Tasks.Common;

namespace TaskManagement.Application.Tasks.Commands.SetTaskCompletion;

/// <summary>Marks a task as completed or active without touching its other fields.</summary>
public record SetTaskCompletionCommand : IRequest<TaskDto>
{
    public int Id { get; init; }

    public bool IsCompleted { get; init; }
}

public class SetTaskCompletionCommandHandler : IRequestHandler<SetTaskCompletionCommand, TaskDto>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetTaskCompletionCommandHandler(ITaskRepository taskRepository, IUnitOfWork unitOfWork)
    {
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TaskDto> Handle(SetTaskCompletionCommand request, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(request.Id, cancellationToken);

        if (task is null)
        {
            throw new NotFoundException(nameof(Domain.Entities.TaskItem), request.Id);
        }

        task.IsCompleted = request.IsCompleted;
        task.UpdatedAtUtc = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return TaskDto.FromEntity(task);
    }
}
