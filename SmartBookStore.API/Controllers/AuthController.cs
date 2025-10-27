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
    public async Task<AuthResponse> Login(LoginRequest request)
    {
        var command = new LoginCommand(request.Username, request.Password);
        var result = await _mediator.Send(command);
        return result.ValueOrDefault;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<AuthResponse> Register(RegisterRequest request)
    {
        var command = new RegisterCommand(request.Username, request.Email, request.Password, request.Role);
        var result = await _mediator.Send(command);
        return result.ValueOrDefault;
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<User> GetProfile()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
        {
            return new User();
        }

        var query = new GetProfileQuery(userId);
        var result = await _mediator.Send(query);
        return result.ValueOrDefault;
    }

    [HttpPost("refresh")]
    [Authorize]
    public async Task<AuthResponse> RefreshToken()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
        {
            return new AuthResponse();
        }

        var command = new RefreshTokenCommand(userId);
        var result = await _mediator.Send(command);
        return result.ValueOrDefault;
    }
}