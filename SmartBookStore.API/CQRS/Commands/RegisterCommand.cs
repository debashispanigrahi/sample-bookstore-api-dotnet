using FluentResults;
using MediatR;
using SmartBookStore.API.Models;
using SmartBookStore.API.Repositories;
using SmartBookStore.API.Services;

namespace SmartBookStore.API.CQRS.Commands;

public record RegisterCommand(string Username, string Email, string Password, string Role = "User") : IRequest<Result<AuthResponse>>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        ITokenService tokenService,
        ILogger<RegisterCommandHandler> logger)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || 
            string.IsNullOrWhiteSpace(request.Email) || 
            string.IsNullOrWhiteSpace(request.Password))
        {
            return Result.Fail<AuthResponse>("Username, email, and password are required");
        }

        if (request.Password.Length < 6)
        {
            return Result.Fail<AuthResponse>("Password must be at least 6 characters long");
        }

        // Check if username already exists
        var existingUserByUsername = await _userRepository.GetByUsernameAsync(request.Username);
        if (existingUserByUsername != null)
        {
            return Result.Fail<AuthResponse>("Username already exists");
        }

        // Check if email already exists
        var existingUserByEmail = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUserByEmail != null)
        {
            return Result.Fail<AuthResponse>("Email already exists");
        }

        // Create new user
        var registerRequest = new RegisterRequest
        {
            Username = request.Username,
            Email = request.Email,
            Password = request.Password,
            Role = request.Role
        };

        var newUser = await _userRepository.CreateUserAsync(registerRequest);

        var token = _tokenService.GenerateToken(newUser);
        var expiresAt = DateTime.UtcNow.AddMinutes(60);

        var response = new AuthResponse
        {
            Token = token,
            Username = newUser.Username,
            Email = newUser.Email,
            Role = newUser.Role,
            ExpiresAt = expiresAt
        };

        _logger.LogInformation("User {Username} registered successfully", newUser.Username);

        return Result.Ok(response);
    }
}