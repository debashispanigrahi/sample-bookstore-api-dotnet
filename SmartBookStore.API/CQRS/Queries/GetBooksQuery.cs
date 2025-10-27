using System.Net;
using FluentResults;
using MediatR;
using SmartBookStore.API.Models;
using SmartBookStore.API.Repositories;

namespace SmartBookStore.API.CQRS.Queries;

public record GetBooksQuery : IRequest<ApiResponse>;

public class GetBooksHandler(IBookRepository repository) : IRequestHandler<GetBooksQuery, ApiResponse>
{
    public async Task<ApiResponse> Handle(GetBooksQuery request, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return new ApiResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                ErrorMessage = "Request was cancelled."
            };
        }

        var books = await repository.GetAllAsync();

        if (books is null)
        {
            return new ApiResponse
            {
                StatusCode = HttpStatusCode.InternalServerError,
                ErrorMessage = "Failed to retrieve books."
            };
        }

        if (!books.Any())
        {
            return new ApiResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                ErrorMessage = "No books found."
            };
        }

        var invalidBook = books.FirstOrDefault(b => b is null
                                                    || string.IsNullOrWhiteSpace(b.Title)
                                                    || b.Price < 0);
        if (invalidBook is not null)
        {
            return new ApiResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                ErrorMessage = "One or more books contain invalid data."
            };
        }

        return new ApiResponse
        {
            Data = books,
            StatusCode = HttpStatusCode.OK
        };
    }
}
