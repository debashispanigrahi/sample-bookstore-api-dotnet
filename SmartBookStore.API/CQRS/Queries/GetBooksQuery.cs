using System.Net;
using MediatR;
using SmartBookStore.API.Models;
using SmartBookStore.API.Repositories;

namespace SmartBookStore.API.CQRS.Queries;

public record GetBooksQuery : IRequest<ApiResponse>;

public class GetBooksHandler(IBookRepository repository) : IRequestHandler<GetBooksQuery, ApiResponse>
{
    public async Task<ApiResponse> Handle(GetBooksQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var books = await repository.GetAllAsync() ?? throw new InvalidOperationException("Failed to retrieve books.");
        if (!books.Any())
        {
            throw new KeyNotFoundException("No books found.");
        }

        var invalidBook = books.FirstOrDefault(b => b is null
                                                    || string.IsNullOrWhiteSpace(b.Title)
                                                    || b.Price < 0);
        if (invalidBook is not null)
        {
            throw new ArgumentException("One or more books contain invalid data.");
        }

        return new ApiResponse { Data = books.ToList(), StatusCode = HttpStatusCode.OK };
    }
}
