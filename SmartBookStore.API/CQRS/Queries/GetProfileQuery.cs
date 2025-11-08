using FluentResults;
using MediatR;
using SmartBookStore.API.Models;
using SmartBookStore.API.Repositories;

namespace SmartBookStore.API.CQRS.Queries;

public record GetProfileQuery(int UserId) : IRequest<Result<User>>;

public class GetProfileQueryHandler(IUserRepository userRepository) : IRequestHandler<GetProfileQuery, Result<User>>
{
    public async Task<Result<User>> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId) ?? throw new KeyNotFoundException("User not found");

        if (user == null) { return Result.Fail("Profile not found."); }

        user.PasswordHash = string.Empty;
        user.Salt = string.Empty;

        return Result.Ok(user);
    }
}