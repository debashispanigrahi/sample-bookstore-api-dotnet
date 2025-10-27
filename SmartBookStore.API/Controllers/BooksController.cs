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
        public async Task<Results<Ok<ApiResponse>, InternalServerError<ApiResponse>, BadRequest<ApiResponse>, NotFound<ApiResponse>>> GetBooks()
        {
            var result = await mediator.Send(new GetBooksQuery());

            if (!result.IsSuccess)
            {
                return result.StatusCode switch
                {
                    HttpStatusCode.BadRequest => TypedResults.BadRequest(result),
                    HttpStatusCode.NotFound => TypedResults.NotFound(result),
                    HttpStatusCode.InternalServerError => TypedResults.InternalServerError(result),
                    _ => TypedResults.BadRequest(result)
                };
            }
            return TypedResults.Ok(result);
        }

        [HttpPost]
        public async Task<Results<Ok<ApiResponse>, InternalServerError<ApiResponse>, BadRequest<ApiResponse>>> CreateBook([FromBody] Book book)
        {
            if (book == null)
            {
                return TypedResults.BadRequest(
                    new ApiResponse { StatusCode = HttpStatusCode.BadRequest, ErrorMessage = "Invalid request" });
            }

            var result = await mediator.Send(new CreateBookCommand { Book = book });

            if (!result.IsSuccess)
            {
                return result.StatusCode switch
                {
                    HttpStatusCode.BadRequest => TypedResults.BadRequest(result),
                    HttpStatusCode.InternalServerError => TypedResults.InternalServerError(result),
                    _ => TypedResults.BadRequest(result)
                };
            }
            return TypedResults.Ok(result);
        }
    }
}