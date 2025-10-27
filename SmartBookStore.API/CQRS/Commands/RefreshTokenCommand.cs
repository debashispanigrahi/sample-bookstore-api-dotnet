using FluentResults;
using MediatR;
using SmartBookStore.API.Models;
using SmartBookStore.API.Repositories;
using SmartBookStore.API.Services;

namespace SmartBookStore.API.CQRS.Commands;

public record RefreshTokenCommand(int UserId) : IRequest<Result<AuthResponse>>;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    public async Task<Result<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            return Result.Fail<AuthResponse>("User not found");
        }

        if (!user.IsActive)
        {
            return Result.Fail<AuthResponse>("Account is disabled");
        }

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

        return Result.Ok(response);
    }
}