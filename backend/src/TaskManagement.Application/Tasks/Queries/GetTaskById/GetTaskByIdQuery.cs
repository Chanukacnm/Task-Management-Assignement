using MediatR;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Tasks.Common;

namespace TaskManagement.Application.Tasks.Queries.GetTaskById;

/// <summary>Returns a single task by id, or 404 if it does not exist.</summary>
public record GetTaskByIdQuery : IRequest<TaskDto>
{
    public int Id { get; init; }
}

public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, TaskDto>
{
    private readonly ITaskRepository _taskRepository;

    public GetTaskByIdQueryHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<TaskDto> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(request.Id, cancellationToken);

        if (task is null)
        {
            throw new NotFoundException(nameof(Domain.Entities.TaskItem), request.Id);
        }

        return TaskDto.FromEntity(task);
    }
}
