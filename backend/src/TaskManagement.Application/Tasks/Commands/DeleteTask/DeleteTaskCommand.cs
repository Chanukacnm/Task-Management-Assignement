using MediatR;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;

namespace TaskManagement.Application.Tasks.Commands.DeleteTask;

/// <summary>Deletes a task by id.</summary>
public record DeleteTaskCommand : IRequest
{
    public int Id { get; init; }
}

public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTaskCommandHandler(ITaskRepository taskRepository, IUnitOfWork unitOfWork)
    {
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(request.Id, cancellationToken);

        if (task is null)
        {
            throw new NotFoundException(nameof(Domain.Entities.TaskItem), request.Id);
        }

        _taskRepository.Remove(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
