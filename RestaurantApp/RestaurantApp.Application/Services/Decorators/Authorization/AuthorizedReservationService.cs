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

    public async Task<Result<ReservationDto>> GetByIdAsync(int reservationId)
    {
        if (!await AuthorizeReservationOperation(reservationId, false))
            return Result<ReservationDto>.Forbidden("You dont have permission to see this reservation.");

        return await _inner.GetByIdAsync(reservationId);
    }

    public async Task<Result<List<ReservationDto>>> GetByRestaurantIdAsync(int restaurantId)
    {
        if (!await AuthorizeInRestaurant(restaurantId))
            return Result<List<ReservationDto>>.Forbidden("You dont have permission to see those reservations.");

        return await _inner.GetByRestaurantIdAsync(restaurantId);
    }

    public async Task<Result<ReservationDto>> CreateAsync(ReservationDto reservationDto)
    {
        if (!await AuthorizeInRestaurant(reservationDto.RestaurantId))
            return Result<ReservationDto>.Forbidden("You dont have permission to create reservations.");
        
        
        return await _inner.CreateAsync(reservationDto);
    }

    public async Task<Result> UpdateAsync(int reservationId, ReservationDto reservationDto)
    {
        if (!await AuthorizeReservationOperation(reservationId, true))
            return Result.Forbidden("You dont have permission to edit this reservation.");

        return await _inner.UpdateAsync(reservationId, reservationDto);
    }

    public async Task<Result> DeleteAsync(int reservationId)
    {
        if (!await AuthorizeReservationOperation(reservationId, true))
            return Result.Forbidden("You dont have permission to delete this reservation.");

        return await _inner.DeleteAsync(reservationId);
    }

    public async Task<Result<PaginatedReservationsDto>> GetUserReservationsAsync(
        ReservationSearchParameters searchParams)
    {
        if(!_currentUser.IsAuthenticated)
            return Result<PaginatedReservationsDto>.Forbidden("User can not see reservations without being logged in");
        
        return await _inner.GetUserReservationsAsync(searchParams);
    }

    public async Task<Result> CancelUserReservationAsync(string userId, int reservationId)
    {
        if (!await AuthorizeReservationOperation(reservationId, false))
            return Result.Forbidden("You dont have permission to cancel this reservation.");
        
        return await _inner.CancelUserReservationAsync(userId, reservationId);
    }

    public async Task<Result<PaginatedReservationsDto>> GetManagedReservationsAsync(string userId, ReservationSearchParameters searchParams)
    {
        if(_currentUser.UserId != userId)
            return Result<PaginatedReservationsDto>.Forbidden("You dont have see this content.");  
        
        return await _inner.GetManagedReservationsAsync(userId, searchParams);
    }

    public async Task<Result> UpdateStatusAsync(int reservationId, ReservationStatusEnumDto status)
    {
        if (!await AuthorizeReservationOperation(reservationId, false))
            return Result.Forbidden("You dont have permission to edit this reserwation.");
        
        return await _inner.UpdateStatusAsync(reservationId, status);
    }

    public async Task<Result<IEnumerable<ReservationDto>>> SearchAsync(ReservationSearchParameters searchParams)
    {
        return await _inner.SearchAsync(searchParams);
    }
    
    private async Task<bool> AuthorizeReservationOperation(int reservationId, bool needToBeEmployee)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.CanManageReservationAsync(_currentUser.UserId!, reservationId, needToBeEmployee);
    }
    
    private async Task<bool> AuthorizeInRestaurant(int restaurantId)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.HasPermissionInRestaurantAsync(_currentUser.UserId!, restaurantId, PermissionType.ManageReservations);
    }
}