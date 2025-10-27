using MediatR;
using SmartBookStore.API.Models;
using SmartBookStore.API.Repositories;
using SmartBookStore.API.Services;
using SmartBookStore.API.Helpers;
using System.Net;

namespace SmartBookStore.API.CQRS.Commands;

public record LoginCommand(string Username, string Password) : IRequest<ApiResponse>;

public class LoginCommandHandler(
    IUserRepository userRepository,
    ITokenService tokenService,
    ILogger<LoginCommandHandler> logger) : IRequestHandler<LoginCommand, ApiResponse>
{
    public async Task<ApiResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return new ApiResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                ErrorMessage = "Username and password are required"
            };
        }

        var user = await userRepository.GetByUsernameAsync(request.Username);
        if (user == null)
        {
            return new ApiResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                ErrorMessage = "Invalid username or password"
            };
        }

        if (!SecurityHelper.VerifyPassword(request.Password, user.Salt, user.PasswordHash))
        {
            return new ApiResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                ErrorMessage = "Invalid username or password"
            };
        }

        if (!user.IsActive)
        {
            return new ApiResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                ErrorMessage = "Account is disabled"
            };
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

        return new ApiResponse
        {
            Data = response,
            StatusCode = HttpStatusCode.OK
        };
    }
}