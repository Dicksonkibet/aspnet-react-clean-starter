using CleanStart.Application.Common.Interfaces;
using CleanStart.Domain.Entities;
using FluentValidation;
using MediatR;

namespace CleanStart.Application.TodoItems;

// Vertical-slice sample feature: every concern for "TodoItems" (DTOs, commands,
// queries, validators, handlers) lives in this one file. This is the pattern to copy
// for each real feature you build — rename the folder/file and go.

public class TodoItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public bool IsDone { get; set; }
}

// ---- Create ----

public record CreateTodoItemCommand(string Title) : IRequest<Guid>;

public class CreateTodoItemCommandValidator : AbstractValidator<CreateTodoItemCommand>
{
    public CreateTodoItemCommandValidator() => RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
}

public class CreateTodoItemHandler : IRequestHandler<CreateTodoItemCommand, Guid>
{
    private readonly IUnitOfWork _uow;
    public CreateTodoItemHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Guid> Handle(CreateTodoItemCommand request, CancellationToken ct)
    {
        var item = new TodoItem { Title = request.Title };
        await _uow.Repository<TodoItem>().AddAsync(item, ct);
        await _uow.SaveChangesAsync(ct);
        return item.Id;
    }
}

// ---- Toggle done ----

public record SetTodoItemDoneCommand(Guid Id, bool IsDone) : IRequest;

public class SetTodoItemDoneHandler : IRequestHandler<SetTodoItemDoneCommand>
{
    private readonly IUnitOfWork _uow;
    public SetTodoItemDoneHandler(IUnitOfWork uow) => _uow = uow;

    public async Task Handle(SetTodoItemDoneCommand request, CancellationToken ct)
    {
        var repo = _uow.Repository<TodoItem>();
        var item = await repo.GetByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException("Todo item not found.");

        item.IsDone = request.IsDone;
        repo.Update(item);
        await _uow.SaveChangesAsync(ct);
    }
}

// ---- Delete ----

public record DeleteTodoItemCommand(Guid Id) : IRequest;

public class DeleteTodoItemHandler : IRequestHandler<DeleteTodoItemCommand>
{
    private readonly IUnitOfWork _uow;
    public DeleteTodoItemHandler(IUnitOfWork uow) => _uow = uow;

    public async Task Handle(DeleteTodoItemCommand request, CancellationToken ct)
    {
        var repo = _uow.Repository<TodoItem>();
        var item = await repo.GetByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException("Todo item not found.");

        repo.Remove(item);
        await _uow.SaveChangesAsync(ct);
    }
}

// ---- List ----

public record GetAllTodoItemsQuery : IRequest<List<TodoItemDto>>;

public class GetAllTodoItemsHandler : IRequestHandler<GetAllTodoItemsQuery, List<TodoItemDto>>
{
    private readonly IUnitOfWork _uow;
    public GetAllTodoItemsHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<List<TodoItemDto>> Handle(GetAllTodoItemsQuery request, CancellationToken ct)
    {
        var items = await _uow.Repository<TodoItem>().ListAsync(ct: ct);
        return items
            .Where(i => !i.IsDeleted)
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new TodoItemDto { Id = i.Id, Title = i.Title, IsDone = i.IsDone })
            .ToList();
    }
}
