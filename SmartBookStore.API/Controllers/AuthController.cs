using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBookStore.API.CQRS.Commands;
using SmartBookStore.API.CQRS.Queries;
using SmartBookStore.API.Models;

namespace SmartBookStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IResult> Login(LoginRequest request)
    {
        if (request == null)
        {
            return TypedResults.BadRequest(ApiResponse<AuthResponse>.Failure(
                "Request body is required",
                System.Net.HttpStatusCode.BadRequest));
        }

        var command = new LoginCommand(request.Username, request.Password);
        var result = await _mediator.Send(command);

        if (result.IsFailed || result.Value == null)
        {
            var err = result.Errors.FirstOrDefault()?.Message ?? "Invalid credentials";
            // Map common auth errors to Unauthorized
            if (err.Contains("Invalid username", StringComparison.OrdinalIgnoreCase)
                || err.Contains("password", StringComparison.OrdinalIgnoreCase)
                || err.Contains("Account is disabled", StringComparison.OrdinalIgnoreCase))
            {
                return Results.Json(ApiResponse<AuthResponse>.Failure(err, System.Net.HttpStatusCode.Unauthorized), statusCode: StatusCodes.Status401Unauthorized);
            }

            return TypedResults.BadRequest(ApiResponse<AuthResponse>.Failure(
                err,
                System.Net.HttpStatusCode.BadRequest));
        }

        return TypedResults.Ok(ApiResponse<AuthResponse>.Success(result.Value));
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IResult> Register(RegisterRequest request)
    {
        if (request == null)
        {
            return TypedResults.BadRequest(ApiResponse<AuthResponse>.Failure(
                "Request body is required",
                System.Net.HttpStatusCode.BadRequest));
        }

        var command = new RegisterCommand(request.Username, request.Email, request.Password, request.Role);
        var result = await _mediator.Send(command);

        if (result.IsFailed || result.Value == null)
        {
            var err = result.Errors.FirstOrDefault()?.Message ?? "Registration failed";
            return TypedResults.BadRequest(ApiResponse<AuthResponse>.Failure(
                err,
                System.Net.HttpStatusCode.BadRequest));
        }

        return TypedResults.Ok(ApiResponse<AuthResponse>.Success(result.Value));
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IResult> GetProfile()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Results.Json(ApiResponse<User>.Failure("Invalid user claim", System.Net.HttpStatusCode.Unauthorized), statusCode: StatusCodes.Status401Unauthorized);
        }

        var query = new GetProfileQuery(userId);
        var result = await _mediator.Send(query);

        if (result.IsFailed || result.Value == null)
        {
            var err = result.Errors.FirstOrDefault()?.Message ?? "Profile not found";
            return TypedResults.BadRequest(ApiResponse<User>.Failure(
                err,
                System.Net.HttpStatusCode.BadRequest));
        }

        return TypedResults.Ok(ApiResponse<User>.Success(result.Value));
    }

    [HttpPost("refresh")]
    [Authorize]
    public async Task<IResult> RefreshToken()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Results.Json(ApiResponse<AuthResponse>.Failure("Invalid user claim", System.Net.HttpStatusCode.Unauthorized), statusCode: StatusCodes.Status401Unauthorized);
        }

        var command = new RefreshTokenCommand(userId);
        var result = await _mediator.Send(command);

        if (result.IsFailed || result.Value == null)
        {
            var err = result.Errors.FirstOrDefault()?.Message ?? "Unable to refresh token";
            return TypedResults.BadRequest(ApiResponse<AuthResponse>.Failure(
                err,
                System.Net.HttpStatusCode.BadRequest));
        }

        return TypedResults.Ok(ApiResponse<AuthResponse>.Success(result.Value));
    }
}