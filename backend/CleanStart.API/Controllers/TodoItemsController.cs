using CleanStart.Application.TodoItems;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanStart.API.Controllers;

/// <summary>Sample controller — thin by design. Every method is one line: send the
/// request to MediatR, return the result. Business logic lives entirely in the
/// Application-layer handlers (see CleanStart.Application/TodoItems).</summary>
[ApiController]
[Route("api/todo-items")]
[Authorize]
public class TodoItemsController : ControllerBase
{
    private readonly ISender _mediator;
    public TodoItemsController(ISender mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<List<TodoItemDto>>> GetAll(CancellationToken ct) =>
        Ok(await _mediator.Send(new GetAllTodoItemsQuery(), ct));

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateTodoItemCommand command, CancellationToken ct) =>
        Ok(await _mediator.Send(command, ct));

    [HttpPatch("{id:guid}/done")]
    public async Task<IActionResult> SetDone(Guid id, [FromBody] bool isDone, CancellationToken ct)
    {
        await _mediator.Send(new SetTodoItemDoneCommand(id, isDone), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteTodoItemCommand(id), ct);
        return NoContent();
    }
}
