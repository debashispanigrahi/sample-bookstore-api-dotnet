using MediatR;
using SmartBookStore.API.Models;
using SmartBookStore.API.Repositories;
using System.Net;

namespace SmartBookStore.API.CQRS.Queries;

public record GetProfileQuery(int UserId) : IRequest<ApiResponse>;

public class GetProfileQueryHandler(IUserRepository userRepository) : IRequestHandler<GetProfileQuery, ApiResponse>
{
    public async Task<ApiResponse> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId) ?? throw new KeyNotFoundException("User not found");

        // Don't return sensitive information
        user.PasswordHash = string.Empty;
        user.Salt = string.Empty;

        return new ApiResponse { Data = user, StatusCode = HttpStatusCode.OK };
    }
}