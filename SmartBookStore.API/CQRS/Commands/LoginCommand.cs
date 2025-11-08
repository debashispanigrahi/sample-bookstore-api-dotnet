using MediatR;
using SmartBookStore.API.Models;
using SmartBookStore.API.Repositories;
using SmartBookStore.API.Services;
using SmartBookStore.API.Helpers;
using System.Net;
using FluentResults;

namespace SmartBookStore.API.CQRS.Commands;

public record LoginCommand(string Username, string Password) : IRequest<Result<AuthResponse>>;

public class LoginCommandHandler(
    IUserRepository userRepository,
    ITokenService tokenService,
    ILogger<LoginCommandHandler> logger) : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Username and password are required");
        }

        var user = await userRepository.GetByUsernameAsync(request.Username) ?? throw new UnauthorizedAccessException("Invalid username or password");
        if (!SecurityHelper.VerifyPassword(request.Password, user.Salt, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("Account is disabled");
        }

        await userRepository.UpdateLastLoginAsync(user.Id);

        var token = tokenService.GenerateToken(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(60);

        var response = new AuthResponse
        {
            Token = token,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            ExpiresAt = expiresAt
        };

        logger.LogInformation("User {Username} logged in successfully", user.Username);

        return Result.Ok(response);
    }
}