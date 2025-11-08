using System.Net;
using FluentResults;
using MediatR;
using SmartBookStore.API.Models;
using SmartBookStore.API.Repositories;

namespace SmartBookStore.API.CQRS.Queries;

public record GetBooksQuery : IRequest<Result<IEnumerable<Book?>>>;
public record GetBookById(int Id) : IRequest<Result<Book?>>;

public class GetBooksHandler(IBookRepository repository) : IRequestHandler<GetBooksQuery, Result<IEnumerable<Book?>>>
{
    public async Task<Result<IEnumerable<Book?>>> Handle(GetBooksQuery request, CancellationToken cancellationToken)
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

        return Result.Ok(books);
    }
}

public class GetBookByIdHandler(IBookRepository repository) : IRequestHandler<GetBookById, Result<Book?>>
{
    public async Task<Result<Book?>> Handle(GetBookById request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var book = await repository.GetByIdAsync(request.Id) ?? throw new InvalidOperationException("Failed to retrieve books.");
        if (book == null)
        {
            throw new KeyNotFoundException("No books found.");
        }

        var invalidBook = string.IsNullOrWhiteSpace(book.Title) || book.Price < 0;
        if (invalidBook)
        {
            throw new ArgumentException("Book contains invalid data.");
        }

        return Result.Ok<Book?>(book);
    }
}
