using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Users;

namespace RestaurantApp.Application.Interfaces.Services;

public interface IUserService
{
    Task<Result<ResponseUserDto>> GetByIdAsync(string id, CancellationToken ct = default);
    Task<Result<IEnumerable<ResponseUserDto>>> SearchAsync(string? firstName,
        string? lastName,
        string? phoneNumber,
        string? email,
        int? amount
        , CancellationToken ct = default);
    Task<Result<CreateUserDto>> CreateAsync(CreateUserDto userDto, CancellationToken ct = default);
    Task<Result> UpdateUserAsync(UpdateUserDto dto, CancellationToken ct = default);
    Task<Result> DeleteUserAsync(string userId, CancellationToken ct = default);
    
    Task<Result<UserDetailsDto>> GetMyDetailsAsync(CancellationToken ct = default);
}