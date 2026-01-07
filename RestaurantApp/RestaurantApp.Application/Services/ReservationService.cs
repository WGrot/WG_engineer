using RestaurantApp.Application.Interfaces;
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

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserNotificationService _userNotificationService;

    private const int DefaultPageSize = 5;
    private const int MaxPageSize = 50;

    public ReservationService(
        IReservationRepository reservationRepository,
        IRestaurantRepository restaurantRepository,
        ICurrentUserService currentUserService,
        IUserNotificationService userNotificationService)
    {
        _reservationRepository = reservationRepository;
        _restaurantRepository = restaurantRepository;
        _currentUserService = currentUserService;
        _userNotificationService = userNotificationService;
    }

    public async Task<Result<ReservationDto>> GetByIdAsync(int reservationId, CancellationToken ct)
    {
        var reservation = await _reservationRepository.GetByIdWithRestaurantAsync(reservationId, ct);

        if (reservation == null)
            return Result<ReservationDto>.NotFound("Reservation not found.");

        return Result<ReservationDto>.Success(reservation.ToDto());
    }

    public async Task<Result<List<ReservationDto>>> GetByRestaurantIdAsync(int restaurantId, CancellationToken ct)
    {
        var reservations = await _reservationRepository.GetByRestaurantIdAsync(restaurantId, ct);
        return Result<List<ReservationDto>>.Success(reservations.ToDtoList());
    }

    public async Task<Result<ReservationDto>> CreateAsync(ReservationDto reservationDto, CancellationToken ct)
    {
        var restaurant = await _restaurantRepository.GetByIdWithSettingsAsync(reservationDto.RestaurantId, ct);

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
            NeedsConfirmation = restaurant!.Settings?.ReservationsNeedConfirmation == true
        };

        var created = await _reservationRepository.AddAsync(reservation, ct);
        return Result<ReservationDto>.Success(created.ToDto());
    }

    public async Task<Result> UpdateAsync(int reservationId, ReservationDto reservationDto, CancellationToken ct)
    {
        var existing = await _reservationRepository.GetByIdAsync(reservationId, ct);

        existing!.UpdateFromDto(reservationDto);
        await _reservationRepository.UpdateAsync(existing!, ct);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int reservationId, CancellationToken ct)
    {
        var existing = await _reservationRepository.GetByIdAsync(reservationId, ct);

        await _reservationRepository.DeleteAsync(existing!, ct);
        return Result.Success();
    }

    public async Task<Result<PaginatedReservationsDto>> GetUserReservationsAsync(
        ReservationSearchParameters searchParams, CancellationToken ct)
    {
        searchParams.UserId = _currentUserService.UserId;

        var (page, pageSize) = NormalizePagination(searchParams.Page, searchParams.PageSize);

        var (items, totalCount) = await _reservationRepository.SearchAsync(searchParams, page, pageSize, ct);

        return Result<PaginatedReservationsDto>.Success(
            CreatePaginatedResult(items, totalCount, page, pageSize));
    }

    public async Task<Result> CancelUserReservationAsync(string userId, int reservationId, CancellationToken ct)
    {
        var reservation = await _reservationRepository.GetByIdAndUserIdAsync(reservationId, userId, ct);

        reservation!.Status = ReservationStatus.Cancelled;
        await _reservationRepository.UpdateAsync(reservation, ct);

        return Result.Success();
    }

    public async Task<Result<PaginatedReservationsDto>> GetManagedReservationsAsync(
        string userId,
        ReservationSearchParameters searchParams, CancellationToken ct)
    {
        var managedRestaurantIds = await _reservationRepository.GetManagedRestaurantIdsAsync(userId, ct);

        if (!managedRestaurantIds.Any())
        {
            return Result<PaginatedReservationsDto>.Success(CreateEmptyPaginatedResult(searchParams));
        }

        searchParams.RestaurantIds = managedRestaurantIds;

        var (page, pageSize) = NormalizePagination(searchParams.Page, searchParams.PageSize);

        var (items, totalCount) = await _reservationRepository.SearchAsync(searchParams, page, pageSize, ct);

        return Result<PaginatedReservationsDto>.Success(
            CreatePaginatedResult(items, totalCount, page, pageSize));
    }

    public async Task<Result> UpdateStatusAsync(int reservationId, ReservationStatusEnumDto status, CancellationToken ct)
    {
        var reservation = await _reservationRepository.GetByIdWithRestaurantAsync(reservationId, ct);

        reservation!.Status = status.ToDomain();
        await _reservationRepository.UpdateAsync(reservation, ct);

        if (!string.IsNullOrEmpty(reservation.UserId))
        {
            await _userNotificationService.CreateAsync(new UserNotification()
            {
                UserId = reservation.UserId!,
                Title = "Reservation update",
                Content = $"Your reservation at '{reservation.Restaurant.Name}' has changed status to {status.ToString().ToLower()}.",
                Type = NotificationType.Info,
                Category = NotificationCategory.ReservationUpdate, 
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            }, ct);
            
        }
        return Result.Success();
    }

    public async Task<Result<IEnumerable<ReservationDto>>> SearchAsync(ReservationSearchParameters searchParams, CancellationToken ct)
    {
        var items = await _reservationRepository.SearchAsync(searchParams, ct);
        return Result<IEnumerable<ReservationDto>>.Success(items.ToDtoList());
    }

    private static (int page, int pageSize) NormalizePagination(int requestedPage, int requestedPageSize)
    {
        var page = requestedPage < 1 ? 1 : requestedPage;
        var pageSize = requestedPageSize < 1 ? DefaultPageSize : Math.Min(requestedPageSize, MaxPageSize);
        return (page, pageSize);
    }

    private static PaginatedReservationsDto CreatePaginatedResult(
        IEnumerable<TableReservation> items,
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
            Reservations = new List<TableReservationDto>(),
            Page = searchParams.Page,
            PageSize = searchParams.PageSize,
            TotalCount = 0,
            TotalPages = 0,
            HasMore = false
        };
    }
}