using RestaurantApp.Application.Interfaces;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Mappers;
using RestaurantApp.Application.Mappers.EnumMappers;
using RestaurantApp.Application.Services.Email.Templates.Reservations;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Reservation;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Application.Services;

public class TableReservationService : ITableReservationService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly ITableRepository _tableRepository;
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IEmailComposer _emailComposer;
    private readonly ICurrentUserService _currentUserService;
    private readonly IRestaurantSettingsRepository _restaurantSettingsRepository;

    public TableReservationService(
        IReservationRepository reservationRepository,
        ITableRepository tableRepository,
        IRestaurantRepository restaurantRepository,
        IEmailComposer emailComposer,
        ICurrentUserService currentUserService,
        IRestaurantSettingsRepository restaurantSettingsRepository)
    {
        _reservationRepository = reservationRepository;
        _tableRepository = tableRepository;
        _restaurantRepository = restaurantRepository;
        _emailComposer = emailComposer;
        _currentUserService = currentUserService;
        _restaurantSettingsRepository = restaurantSettingsRepository;
    }

    public async Task<Result<TableReservationDto>> GetByIdAsync(int reservationId)
    {
        var reservation = await _reservationRepository.GetTableReservationByIdWithDetailsAsync(reservationId);

        if (reservation == null)
            return Result<TableReservationDto>.NotFound("Table reservation not found");

        return Result<TableReservationDto>.Success(reservation.ToTableReservationDto());
    }

    public async Task<Result<IEnumerable<ReservationDto>>> GetByTableIdAsync(int tableId)
    {
        var reservations = await _reservationRepository.GetTableReservationsByTableIdAsync(tableId);
        return Result<IEnumerable<ReservationDto>>.Success(reservations.ToDtoList());
    }

    public async Task<Result<TableReservationDto>> CreateAsync(CreateTableReservationDto dto)
    {
        var restaurantSettings = await _restaurantSettingsRepository.GetByRestaurantIdAsync(dto.RestaurantId);
        bool needsConfirmation = restaurantSettings!.ReservationsNeedConfirmation;
        var initialStatus = needsConfirmation
            ? ReservationStatusEnumDto.Pending
            : ReservationStatusEnumDto.Confirmed;

        string? userId = null;
        if (dto.UseUserId)
        {
            userId = _currentUserService.UserId;
        }

        var reservation = CreateReservation(dto, userId, initialStatus, needsConfirmation);

        var created = await _reservationRepository.AddTableReservationAsync(reservation);

        await SendReservationNotificationAsync(created, needsConfirmation);

        return Result<TableReservationDto>.Success(created.ToTableReservationDto());
    }

    public async Task<Result> UpdateAsync(int reservationId, TableReservationDto dto)
    {
        var existing = await _reservationRepository.GetTableReservationByIdAsync(reservationId);

        UpdateReservationFromDto(existing!, dto);

        await _reservationRepository.UpdateTableReservationAsync(existing!);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int reservationId)
    {
        var existing = await _reservationRepository.GetTableReservationByIdAsync(reservationId);

        await _reservationRepository.DeleteAsync(existing!);
        return Result.Success();
    }

    private static void UpdateReservationFromDto(TableReservation reservation, TableReservationDto dto)
    {
        reservation.RestaurantId = dto.RestaurantId;
        reservation.UserId = dto.UserId;
        reservation.NumberOfGuests = dto.NumberOfGuests;
        reservation.CustomerName = dto.CustomerName;
        reservation.CustomerEmail = dto.CustomerEmail;
        reservation.CustomerPhone = dto.CustomerPhone;
        reservation.ReservationDate = dto.ReservationDate;
        reservation.StartTime = dto.StartTime;
        reservation.EndTime = dto.EndTime;
        reservation.Notes = dto.Notes;
        reservation.TableId = dto.TableId;
    }

    private async Task SendReservationNotificationAsync(TableReservation reservation, bool needsConfirmation)
    {
        if (needsConfirmation)
        {
            var email = new ReservationCreatedEmail(reservation);
            await _emailComposer.SendAsync(reservation.CustomerEmail, email);
        }
        else
        {
            var email = new ReservationConfirmedEmail(reservation);
            await _emailComposer.SendAsync(reservation.CustomerEmail, email);
        }
    }
    
    private TableReservation CreateReservation(
        CreateTableReservationDto dto,
        string? userId,
        ReservationStatusEnumDto initialStatus,
        bool needsConfirmation)
    {
        return new TableReservation
        {
            RestaurantId = dto.RestaurantId,
            UserId = userId,
            NumberOfGuests = dto.NumberOfGuests,
            CustomerName = dto.CustomerName,
            CustomerEmail = dto.CustomerEmail,
            CustomerPhone = dto.CustomerPhone,
            ReservationDate = dto.ReservationDate,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Notes = dto.Notes,
            TableId = dto.TableId,
            Status = initialStatus.ToDomain(),
            CreatedAt = DateTime.UtcNow,
            NeedsConfirmation = needsConfirmation
        };
    }
}