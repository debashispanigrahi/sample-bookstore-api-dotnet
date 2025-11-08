using FluentResults;
using MediatR;
using SmartBookStore.API.CQRS.Queries;
using SmartBookStore.API.Models;
using SmartBookStore.API.Repositories;

namespace SmartBookStore.API.CQRS.Commands;

public record CreateBookCommand : IRequest<Result<int>>
{
    public required Book Book { get; set; }
}

public class CreateBookHandler(IBookRepository repository) : IRequestHandler<CreateBookCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateBookCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var newId = await repository.CreateAsync(request.Book);

        if (newId < 1)
        {
            throw new InvalidOperationException("Failed to insert book.");
        }

        return Result.Ok(newId);
    }
}

