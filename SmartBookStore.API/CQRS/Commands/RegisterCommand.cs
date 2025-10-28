using MediatR;
using SmartBookStore.API.Models;
using SmartBookStore.API.Repositories;
using SmartBookStore.API.Services;
using System.Net;

namespace SmartBookStore.API.CQRS.Commands;

public record RegisterCommand(string Username, string Email, string Password, string Role = "User") : IRequest<ApiResponse>;

public class RegisterCommandHandler(
    IUserRepository userRepository,
    ITokenService tokenService,
    ILogger<RegisterCommandHandler> logger) : IRequestHandler<RegisterCommand, ApiResponse>
{
    public async Task<ApiResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || 
            string.IsNullOrWhiteSpace(request.Email) || 
            string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Username, email, and password are required");
        }

        if (request.Password.Length < 6)
        {
            throw new ArgumentException("Password must be at least 6 characters long");
        }

        var existingUserByUsername = await userRepository.GetByUsernameAsync(request.Username);
        if (existingUserByUsername != null)
        {
            throw new ArgumentException("Username already exists");
        }

        var existingUserByEmail = await userRepository.GetByEmailAsync(request.Email);
        if (existingUserByEmail != null)
        {
            throw new ArgumentException("Email already exists");
        }

        var registerRequest = new RegisterRequest
        {
            Username = request.Username,
            Email = request.Email,
            Password = request.Password,
            Role = request.Role
        };

        var newUser = await userRepository.CreateUserAsync(registerRequest);

        var token = tokenService.GenerateToken(newUser);
        var expiresAt = DateTime.UtcNow.AddMinutes(60);

        var response = new AuthResponse
        {
            Token = token,
            Username = newUser.Username,
            Email = newUser.Email,
            Role = newUser.Role,
            ExpiresAt = expiresAt
        };

        logger.LogInformation("User {Username} registered successfully", newUser.Username);

        return new ApiResponse { Data = response, StatusCode = System.Net.HttpStatusCode.OK };
    }
}