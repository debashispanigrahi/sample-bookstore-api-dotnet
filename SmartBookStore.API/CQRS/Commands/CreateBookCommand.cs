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
        try
        {
            var newId = await repository.CreateAsync(request.Book);
            return Result.Ok(newId);
        }
        catch (ArgumentException ex)
        {
            return Result.Fail<int>(ex.Message);
        }
        catch (Exception ex)
        {
            return Result.Fail<int>($"An error occurred while creating the book: {ex.Message}");
        }
    }
}

