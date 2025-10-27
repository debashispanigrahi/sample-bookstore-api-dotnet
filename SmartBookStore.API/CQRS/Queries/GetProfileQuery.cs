using FluentResults;
using MediatR;
using SmartBookStore.API.Models;
using SmartBookStore.API.Repositories;

namespace SmartBookStore.API.CQRS.Queries;

public record GetProfileQuery(int UserId) : IRequest<Result<User>>;

public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, Result<User>>
{
    private readonly IUserRepository _userRepository;

    public GetProfileQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<User>> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            return Result.Fail<User>("User not found");
        }

        // Don't return sensitive information
        user.PasswordHash = string.Empty;
        user.Salt = string.Empty;

        return Result.Ok(user);
    }
}