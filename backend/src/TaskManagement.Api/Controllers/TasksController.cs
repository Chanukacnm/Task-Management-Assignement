using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Tasks.Commands.CreateTask;
using TaskManagement.Application.Tasks.Commands.DeleteTask;
using TaskManagement.Application.Tasks.Commands.SetTaskCompletion;
using TaskManagement.Application.Tasks.Commands.UpdateTask;
using TaskManagement.Application.Tasks.Common;
using TaskManagement.Application.Tasks.Queries.GetTaskById;
using TaskManagement.Application.Tasks.Queries.GetTasks;

namespace TaskManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class TasksController : ControllerBase
{
    private readonly ISender _mediator;

    public TasksController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Returns all tasks, optionally filtered and sorted.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TaskDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TaskDto>>> GetTasks([FromQuery] GetTasksQuery query)
    {
        var tasks = await _mediator.Send(query);
        return Ok(tasks);
    }

    /// <summary>Returns a single task by id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDto>> GetTask(int id)
    {
        var task = await _mediator.Send(new GetTaskByIdQuery { Id = id });
        return Ok(task);
    }

    /// <summary>Creates a new task.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskCommand command)
    {
        var created = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetTask), new { id = created.Id }, created);
    }

    /// <summary>Updates an existing task.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDto>> UpdateTask(int id, [FromBody] UpdateTaskCommand command)
    {
        // The route id is authoritative; ignore any id sent in the body.
        var updated = await _mediator.Send(command with { Id = id });
        return Ok(updated);
    }

    /// <summary>Marks a task as completed or active.</summary>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDto>> SetTaskStatus(int id, [FromBody] SetTaskCompletionCommand command)
    {
        var updated = await _mediator.Send(command with { Id = id });
        return Ok(updated);
    }

    /// <summary>Deletes a task.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask(int id)
    {
        await _mediator.Send(new DeleteTaskCommand { Id = id });
        return NoContent();
    }
}
