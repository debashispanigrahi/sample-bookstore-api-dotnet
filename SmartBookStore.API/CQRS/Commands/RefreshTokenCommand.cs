using MediatR;
using SmartBookStore.API.Models;
using SmartBookStore.API.Repositories;
using SmartBookStore.API.Services;
using System.Net;

namespace SmartBookStore.API.CQRS.Commands;

public record RefreshTokenCommand(int UserId) : IRequest<ApiResponse>;

public class RefreshTokenCommandHandler(
    IUserRepository userRepository,
    ITokenService tokenService) : IRequestHandler<RefreshTokenCommand, ApiResponse>
{
    public async Task<ApiResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            return new ApiResponse { StatusCode = HttpStatusCode.NotFound, ErrorMessage = "User not found" };
        }

        if (!user.IsActive)
        {
            return new ApiResponse { StatusCode = HttpStatusCode.Unauthorized, ErrorMessage = "Account is disabled" };
        }

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

        return new ApiResponse { Data = response, StatusCode = HttpStatusCode.OK };
    }
}