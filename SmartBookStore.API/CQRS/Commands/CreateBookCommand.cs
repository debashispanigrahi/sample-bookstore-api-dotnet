using System;
using FluentResults;
using MediatR;
using SmartBookStore.API.Models;
using SmartBookStore.API.Repositories;

namespace SmartBookStore.API.CQRS.Commands;

public record CreateBookCommand(Book Book) : IRequest<Result<int>>;
public class CreateBookHandler(IBookRepository repository) : IRequestHandler<CreateBookCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateBookCommand request, CancellationToken cancellationToken)
    {
        var book = request.Book;
        return await repository.CreateAsync(book);
    }
}

