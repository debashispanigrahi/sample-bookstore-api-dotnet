using Asp.Versioning;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartBookStore.API.CQRS.Commands;
using SmartBookStore.API.CQRS.Queries;
using SmartBookStore.API.Models;
using System.Net;

namespace SmartBookStore.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    public class BooksController(IMediator mediator) : ControllerBase
    {
        [HttpGet]
        public async Task<IResult> GetBooks()
        {
            var result = await mediator.Send(new GetBooksQuery());

            if (result.IsFailed)
            {
                return TypedResults.BadRequest(ApiResponse<IEnumerable<Book>>.Failure(
                    result.Errors.First().Message,
                    HttpStatusCode.BadRequest));
            }

            if (result.Value == null)
            {
                return TypedResults.NotFound(ApiResponse<IEnumerable<Book>>.Failure(
                    "No books found",
                    HttpStatusCode.NotFound));
            }

            return TypedResults.Ok(ApiResponse<IEnumerable<Book>>.Success(result.Value));
        }

        [HttpPost]
        public async Task<IResult> CreateBook([FromBody] Book book)
        {
            if (book == null)
            {
                return TypedResults.BadRequest(ApiResponse<int>.Failure(
                    "Book data is required",
                    HttpStatusCode.BadRequest));
            }

            var result = await mediator.Send(new CreateBookCommand(book));

            if (result.IsFailed)
            {
                return TypedResults.BadRequest(ApiResponse<int>.Failure(
                    result.Errors.First().Message,
                    HttpStatusCode.BadRequest));
            }

            return TypedResults.Ok(ApiResponse<int>.Success(result.Value));
        }
    }
}