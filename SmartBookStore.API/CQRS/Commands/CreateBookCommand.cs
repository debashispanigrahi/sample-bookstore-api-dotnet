using FluentResults;
using MediatR;
using SmartBookStore.API.CQRS.Queries;
using SmartBookStore.API.Models;
using SmartBookStore.API.Repositories;

namespace SmartBookStore.API.CQRS.Commands;

public record CreateBookCommand : IRequest<ApiResponse>
{
    public required Book Book { get; set; }
}

public class CreateBookHandler(IBookRepository repository) : IRequestHandler<CreateBookCommand, ApiResponse>
{
    public async Task<ApiResponse> Handle(CreateBookCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var newId = await repository.CreateAsync(request.Book);

        if (newId < 1)
        {
            throw new InvalidOperationException("Failed to insert book.");
        }

        return new ApiResponse { Data = newId, StatusCode = System.Net.HttpStatusCode.OK };
    }
}

