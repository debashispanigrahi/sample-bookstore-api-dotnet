using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBookStore.API.CQRS.Commands;
using SmartBookStore.API.CQRS.Queries;
using SmartBookStore.API.Models;
using System.Net;

namespace SmartBookStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IResult> Login(LoginRequest request)
    {
        if (request == null)
        {
            return TypedResults.BadRequest(new ApiResponse { StatusCode = HttpStatusCode.BadRequest, ErrorMessage = "Request body is required" });
        }

        var command = new LoginCommand(request.Username, request.Password);
        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            return result.StatusCode switch
            {
                HttpStatusCode.Unauthorized => Results.Json(result, statusCode: StatusCodes.Status401Unauthorized),
                HttpStatusCode.BadRequest => TypedResults.BadRequest(result),
                _ => TypedResults.BadRequest(result)
            };
        }

        return TypedResults.Ok(result);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IResult> Register(RegisterRequest request)
    {
        if (request == null)
        {
            return TypedResults.BadRequest(new ApiResponse { StatusCode = HttpStatusCode.BadRequest, ErrorMessage = "Request body is required" });
        }

        var command = new RegisterCommand(request.Username, request.Email, request.Password, request.Role);
        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            return TypedResults.BadRequest(result);
        }

        return TypedResults.Ok(result);
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IResult> GetProfile()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Results.Json(new ApiResponse { StatusCode = HttpStatusCode.Unauthorized, ErrorMessage = "Invalid user claim" }, statusCode: StatusCodes.Status401Unauthorized);
        }

        var query = new GetProfileQuery(userId);
        var result = await mediator.Send(query);

        if (!result.IsSuccess)
        {
            return TypedResults.BadRequest(result);
        }

        return TypedResults.Ok(result);
    }

    [HttpPost("refresh")]
    [Authorize]
    public async Task<IResult> RefreshToken()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Results.Json(new ApiResponse { StatusCode = HttpStatusCode.Unauthorized, ErrorMessage = "Invalid user claim" }, statusCode: StatusCodes.Status401Unauthorized);
        }

        var command = new RefreshTokenCommand(userId);
        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            return TypedResults.BadRequest(result);
        }

        return TypedResults.Ok(result);
    }
}