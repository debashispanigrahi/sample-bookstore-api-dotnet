using FluentResults;
using MediatR;
using SmartBookStore.API.Models;
using SmartBookStore.API.Repositories;
using SmartBookStore.API.Services;
using SmartBookStore.API.Helpers;

namespace SmartBookStore.API.CQRS.Commands;

public record LoginCommand(string Username, string Password) : IRequest<Result<AuthResponse>>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUserRepository userRepository,
        ITokenService tokenService,
        ILogger<LoginCommandHandler> logger)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Result.Fail<AuthResponse>("Username and password are required");
        }

        var user = await _userRepository.GetByUsernameAsync(request.Username);
        if (user == null)
        {
            return Result.Fail<AuthResponse>("Invalid username or password");
        }

        if (!SecurityHelper.VerifyPassword(request.Password, user.Salt, user.PasswordHash))
        {
            return Result.Fail<AuthResponse>("Invalid username or password");
        }

        if (!user.IsActive)
        {
            return Result.Fail<AuthResponse>("Account is disabled");
        }

        await _userRepository.UpdateLastLoginAsync(user.Id);

        var token = _tokenService.GenerateToken(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(60);

        var response = new AuthResponse
        {
            Token = token,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            ExpiresAt = expiresAt
        };

        _logger.LogInformation("User {Username} logged in successfully", user.Username);

        return Result.Ok(response);
    }
}