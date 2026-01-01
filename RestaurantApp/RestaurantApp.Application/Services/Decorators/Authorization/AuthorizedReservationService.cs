using RestaurantApp.Application.Interfaces;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Domain.Enums;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Reservation;
using RestaurantApp.Shared.DTOs.SearchParameters;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Application.Services.Decorators.Authorization;

public class AuthorizedReservationService : IReservationService
{
    private readonly IReservationService _inner;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthorizationChecker _authorizationChecker;

    public AuthorizedReservationService(
        IReservationService inner,
        ICurrentUserService currentUser,
        IAuthorizationChecker authorizationChecker)
    {
        _inner = inner;
        _currentUser = currentUser;
        _authorizationChecker = authorizationChecker;
    }

    public async Task<Result<ReservationDto>> GetByIdAsync(int reservationId, CancellationToken ct)
    {
        if (!await AuthorizeReservationOperation(reservationId, false, ct))
            return Result<ReservationDto>.Forbidden("You dont have permission to see this reservation.");

        return await _inner.GetByIdAsync(reservationId, ct);
    }

    public async Task<Result<List<ReservationDto>>> GetByRestaurantIdAsync(int restaurantId, CancellationToken ct)
    {
        if (!await AuthorizeInRestaurant(restaurantId, ct))
            return Result<List<ReservationDto>>.Forbidden("You dont have permission to see those reservations.");

        return await _inner.GetByRestaurantIdAsync(restaurantId, ct);
    }

    public async Task<Result<ReservationDto>> CreateAsync(ReservationDto reservationDto, CancellationToken ct)
    {
        if (!await AuthorizeInRestaurant(reservationDto.RestaurantId, ct))
            return Result<ReservationDto>.Forbidden("You dont have permission to create reservations.");
        
        
        return await _inner.CreateAsync(reservationDto, ct);
    }

    public async Task<Result> UpdateAsync(int reservationId, ReservationDto reservationDto, CancellationToken ct)
    {
        if (!await AuthorizeReservationOperation(reservationId, true, ct))
            return Result.Forbidden("You dont have permission to edit this reservation.");

        return await _inner.UpdateAsync(reservationId, reservationDto, ct);
    }

    public async Task<Result> DeleteAsync(int reservationId, CancellationToken ct)
    {
        if (!await AuthorizeReservationOperation(reservationId, true, ct))
            return Result.Forbidden("You dont have permission to delete this reservation.");

        return await _inner.DeleteAsync(reservationId, ct);
    }

    public async Task<Result<PaginatedReservationsDto>> GetUserReservationsAsync(
        ReservationSearchParameters searchParams, CancellationToken ct)
    {
        if(!_currentUser.IsAuthenticated)
            return Result<PaginatedReservationsDto>.Forbidden("User can not see reservations without being logged in");
        
        return await _inner.GetUserReservationsAsync(searchParams, ct);
    }

    public async Task<Result> CancelUserReservationAsync(string userId, int reservationId, CancellationToken ct)
    {
        if (!await AuthorizeReservationOperation(reservationId, false, ct))
            return Result.Forbidden("You dont have permission to cancel this reservation.");
        
        return await _inner.CancelUserReservationAsync(userId, reservationId, ct);
    }

    public async Task<Result<PaginatedReservationsDto>> GetManagedReservationsAsync(string userId, ReservationSearchParameters searchParams, CancellationToken ct)
    {
        if(_currentUser.UserId != userId)
            return Result<PaginatedReservationsDto>.Forbidden("You dont have see this content.");  
        
        return await _inner.GetManagedReservationsAsync(userId, searchParams, ct);
    }

    public async Task<Result> UpdateStatusAsync(int reservationId, ReservationStatusEnumDto status, CancellationToken ct)
    {
        if (!await AuthorizeReservationOperation(reservationId, false, ct))
            return Result.Forbidden("You dont have permission to edit this reserwation.");
        
        return await _inner.UpdateStatusAsync(reservationId, status, ct);
    }

    public async Task<Result<IEnumerable<ReservationDto>>> SearchAsync(ReservationSearchParameters searchParams, CancellationToken ct)
    {
        return await _inner.SearchAsync(searchParams, ct);
    }
    
    private async Task<bool> AuthorizeReservationOperation(int reservationId, bool needToBeEmployee, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.CanManageReservationAsync(_currentUser.UserId!, reservationId, needToBeEmployee);
    }
    
    private async Task<bool> AuthorizeInRestaurant(int restaurantId, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.HasPermissionInRestaurantAsync(_currentUser.UserId!, restaurantId, PermissionType.ManageReservations);
    }
}