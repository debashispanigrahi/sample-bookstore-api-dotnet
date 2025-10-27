using FluentResults;
using MediatR;
using SmartBookStore.API.CQRS.Queries;
using SmartBookStore.API.Models;
using SmartBookStore.API.Repositories;
using System.Net;

namespace SmartBookStore.API.CQRS.Commands;

public record CreateBookCommand : IRequest<ApiResponse>
{
    public required Book Book { get; set; }
}

public class CreateBookHandler(IBookRepository repository) : IRequestHandler<CreateBookCommand, ApiResponse>
{
    public async Task<ApiResponse> Handle(CreateBookCommand request, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return new ApiResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                ErrorMessage = "Request was cancelled."
            };
        }

        var newId = await repository.CreateAsync(request.Book);

        if (newId < 1)
        {
            return new ApiResponse
            {
                StatusCode = HttpStatusCode.InternalServerError,
                ErrorMessage = "Failed to insert books."
            };
        }

        return new ApiResponse
        {
            Data = newId,
            StatusCode = HttpStatusCode.OK
        };
    }
}

