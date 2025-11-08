using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
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
        public async Task<Results<Ok<ApiResponse>, NotFound<ApiResponse>, BadRequest<ApiResponse>>> GetBooks()
        {
            var books = await mediator.Send(new GetBooksQuery());
            var apiResponse = new ApiResponse { Data = books.Value, StatusCode = HttpStatusCode.OK };
            return TypedResults.Ok(apiResponse);
        }

        [HttpGet("{Id:int}")]
        public async Task<Results<Ok<ApiResponse>, NotFound<ApiResponse>, BadRequest<ApiResponse>>> GetBookById(int Id)
        {
            var book = await mediator.Send(new GetBookById(Id));
            var apiResponse = new ApiResponse { Data = book.Value, StatusCode = HttpStatusCode.OK };
            return TypedResults.Ok(apiResponse);
        }

        [HttpPost]
        public async Task<Results<Created<ApiResponse>, BadRequest<ApiResponse>>> CreateBook([FromBody] Book book)
        {
            if (string.IsNullOrWhiteSpace(book.Title) || string.IsNullOrWhiteSpace(book.Author) || string.IsNullOrWhiteSpace(book.Isbn))
            {
                return TypedResults.BadRequest(new ApiResponse { StatusCode = HttpStatusCode.BadRequest, ErrorMessage = "Invalid request" });
            }

            var bookId = await mediator.Send(new CreateBookCommand { Book = book });
            var apiResponse = new ApiResponse { Data = bookId.Value, StatusCode = HttpStatusCode.OK };
            return TypedResults.Created($"/api/v1/books/{bookId.Value}", apiResponse);
        }
    }
}