using Asp.Versioning;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
            var apiResponse = new ApiResponse { Data = books, StatusCode = HttpStatusCode.OK };
            return TypedResults.Ok(apiResponse);
        }

        [HttpPost]
        public async Task<Results<Created<ApiResponse>, BadRequest<ApiResponse>>> CreateBook([FromBody] Book book)
        {
            if (book == null)
            {
                return TypedResults.BadRequest(new ApiResponse { StatusCode = HttpStatusCode.BadRequest, ErrorMessage = "Invalid request" });
            }

            var bookId = await mediator.Send(new CreateBookCommand { Book = book });
            var apiResponse = new ApiResponse { Data = bookId, StatusCode = HttpStatusCode.OK };
            return TypedResults.Created($"/api/v1/books/{bookId}", apiResponse);
        }
    }
}