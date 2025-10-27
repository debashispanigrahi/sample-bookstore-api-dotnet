using System;
using FluentResults;
using MediatR;
using SmartBookStore.API.Models;
using SmartBookStore.API.Repositories;

namespace SmartBookStore.API.CQRS.Queries;

public record GetBooksQuery : IRequest<Result<IEnumerable<Book>>>;


public class GetBooksHandler(IBookRepository repository) : IRequestHandler<GetBooksQuery, Result<IEnumerable<Book>>>
{
    public async Task<Result<IEnumerable<Book>>> Handle(GetBooksQuery request, CancellationToken cancellationToken)
    {
        return await repository.GetAllAsync();
    }
}
