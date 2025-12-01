using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Mappers;
using RestaurantApp.Application.Mappers.EnumMappers;
using RestaurantApp.Domain.Enums;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Reservation;
using RestaurantApp.Shared.DTOs.SearchParameters;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Application.Services;

public class ReservationService: IReservationService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IRestaurantRepository _restaurantRepository;

    private const int DefaultPageSize = 5;
    private const int MaxPageSize = 50;

    public ReservationService(
        IReservationRepository reservationRepository,
        IRestaurantRepository restaurantRepository)
    {
        _reservationRepository = reservationRepository;
        _restaurantRepository = restaurantRepository;
    }
    

    public async Task<Result<ReservationDto>> GetByIdAsync(int reservationId)
    {
        var reservation = await _reservationRepository.GetByIdWithRestaurantAsync(reservationId);

        if (reservation == null)
            return Result<ReservationDto>.NotFound("Reservation not found");

        return Result<ReservationDto>.Success(reservation.ToDto());
    }

    public async Task<Result<List<ReservationDto>>> GetByRestaurantIdAsync(int restaurantId)
    {
        var reservations = await _reservationRepository.GetByRestaurantIdAsync(restaurantId);
        return Result<List<ReservationDto>>.Success(reservations.ToDtoList());
    }

    public async Task<Result<ReservationDto>> CreateAsync(ReservationDto reservationDto)
    {
        var restaurant = await _restaurantRepository.GetByIdWithSettingsAsync(reservationDto.RestaurantId);

        if (restaurant == null)
            return Result<ReservationDto>.NotFound($"Restaurant {reservationDto.RestaurantId} not found");

        var reservation = new ReservationBase
        {
            RestaurantId = reservationDto.RestaurantId,
            UserId = reservationDto.UserId,
            NumberOfGuests = reservationDto.NumberOfGuests,
            CustomerName = reservationDto.CustomerName,
            CustomerEmail = reservationDto.CustomerEmail,
            CustomerPhone = reservationDto.CustomerPhone,
            ReservationDate = DateTime.UtcNow,
            StartTime = reservationDto.StartTime,
            EndTime = reservationDto.EndTime,
            Notes = reservationDto.Notes,
            Status = ReservationStatusEnumDto.Pending.ToDomain(),
            CreatedAt = DateTime.UtcNow,
            NeedsConfirmation = restaurant.Settings?.ReservationsNeedConfirmation == true
        };

        var created = await _reservationRepository.AddAsync(reservation);
        return Result<ReservationDto>.Success(created.ToDto());
    }

    public async Task<Result> UpdateAsync(int reservationId, ReservationDto reservationDto)
    {
        var existing = await _reservationRepository.GetByIdAsync(reservationId);

        if (existing == null)
            return Result.NotFound($"Reservation {reservationId} not found");

        existing.UpdateFromDto(reservationDto);
        await _reservationRepository.UpdateAsync(existing);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int reservationId)
    {
        var existing = await _reservationRepository.GetByIdAsync(reservationId);

        if (existing == null)
            return Result.NotFound($"Reservation {reservationId} not found");

        await _reservationRepository.DeleteAsync(existing);
        return Result.Success();
    }
    
    public async Task<Result<PaginatedReservationsDto>> GetUserReservationsAsync(
        string userId,
        ReservationSearchParameters searchParams)
    {
        if (string.IsNullOrEmpty(userId))
            return Result<PaginatedReservationsDto>.Failure("User ID is required.");
        
        searchParams.UserId = userId;

        var (page, pageSize) = NormalizePagination(searchParams.Page, searchParams.PageSize);

        var (items, totalCount) = await _reservationRepository.SearchAsync(searchParams, page, pageSize);

        return Result<PaginatedReservationsDto>.Success(
            CreatePaginatedResult(items, totalCount, page, pageSize));
    }

    public async Task<Result> CancelUserReservationAsync(string userId, int reservationId)
    {
        if (string.IsNullOrEmpty(userId))
            return Result.Failure("User ID is required.", 400);

        var reservation = await _reservationRepository.GetByIdAndUserIdAsync(reservationId, userId);

        if (reservation == null)
            return Result.Failure("Reservation not found or does not belong to the user", 404);

        reservation.Status = ReservationStatus.Cancelled;
        await _reservationRepository.UpdateAsync(reservation);

        return Result.Success();
    }
    

    public async Task<Result<PaginatedReservationsDto>> GetManagedReservationsAsync(
        string userId,
        ReservationSearchParameters searchParams)
    {
        if (string.IsNullOrEmpty(userId))
            return Result<PaginatedReservationsDto>.Failure("User ID is required.");

        var managedRestaurantIds = await _reservationRepository.GetManagedRestaurantIdsAsync(userId);

        if (!managedRestaurantIds.Any())
        {
            return Result<PaginatedReservationsDto>.Success(CreateEmptyPaginatedResult(searchParams));
        }
        
        searchParams.RestaurantIds = managedRestaurantIds;

        var (page, pageSize) = NormalizePagination(searchParams.Page, searchParams.PageSize);

        var (items, totalCount) = await _reservationRepository.SearchAsync(searchParams, page, pageSize);

        return Result<PaginatedReservationsDto>.Success(
            CreatePaginatedResult(items, totalCount, page, pageSize));
    }

    public async Task<Result> UpdateStatusAsync(int reservationId, ReservationStatusEnumDto status)
    {
        var reservation = await _reservationRepository.GetByIdAsync(reservationId);

        if (reservation == null)
            return Result.NotFound($"Reservation {reservationId} not found");

        reservation.Status = status.ToDomain();
        await _reservationRepository.UpdateAsync(reservation);

        return Result.Success();
    }
    

    public async Task<Result<IEnumerable<ReservationDto>>> SearchAsync(ReservationSearchParameters searchParams)
    {
        var items = await _reservationRepository.SearchAsync(searchParams);
        return Result<IEnumerable<ReservationDto>>.Success(items.ToDtoList());
    }

    private static (int page, int pageSize) NormalizePagination(int requestedPage, int requestedPageSize)
    {
        var page = requestedPage < 1 ? 1 : requestedPage;
        var pageSize = requestedPageSize < 1 ? DefaultPageSize : Math.Min(requestedPageSize, MaxPageSize);
        return (page, pageSize);
    }

    private static PaginatedReservationsDto CreatePaginatedResult(
        IEnumerable<ReservationBase> items,
        int totalCount,
        int page,
        int pageSize)
    {
        return new PaginatedReservationsDto
        {
            Reservations = items.ToDtoList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
            HasMore = page * pageSize < totalCount
        };
    }

    private static PaginatedReservationsDto CreateEmptyPaginatedResult(ReservationSearchParameters searchParams)
    {
        return new PaginatedReservationsDto
        {
            Reservations = new List<ReservationDto>(),
            Page = searchParams.Page,
            PageSize = searchParams.PageSize,
            TotalCount = 0,
            TotalPages = 0,
            HasMore = false
        };
    }
}