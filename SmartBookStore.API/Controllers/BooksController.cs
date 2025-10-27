using Asp.Versioning;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBookStore.API.CQRS.Commands;
using SmartBookStore.API.CQRS.Queries;
using SmartBookStore.API.Models;

namespace SmartBookStore.API.Controllers
{

    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    public class BooksController(IMediator mediator) : ControllerBase
    {
        [HttpGet]
        public async Task<IEnumerable<Book>> GetBooks()
        {
            var result = await mediator.Send(new GetBooksQuery());
            return result.ValueOrDefault;
        }

        [HttpPost]
        public async Task<int> CreateBook([FromBody] Book book)
        {
            if (book == null)
            {
                return 0;
            }

            var result = await mediator.Send(new CreateBookCommand(book));
            return result.ValueOrDefault;
        }
    }
}