﻿using RestaurantApp.Api.Common;
using RestaurantApp.Api.Models;
using RestaurantApp.Api.Models.DTOs;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.SearchParameters;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services.Interfaces;

public interface IReservationService
{
    Task<Result<ReservationBase>> GetReservationByIdAsync(int reservationId);
    Task<Result<IEnumerable<ReservationBase>>> GetReservationsByRestaurantIdAsync(int restaurantId);
    
    Task<Result<IEnumerable<ReservationBase>>> GetReservationsByUserIdAsync(ReservationSearchParameters searchParams);
    Task<Result<ReservationBase>> CreateReservationAsync(ReservationDto reservationDto);
    Task<Result> UpdateReservationAsync(int reservationId, ReservationDto reservationDto);
    Task<Result> DeleteReservationAsync(int reservationId);
    
    Task<Result<List<ReservationBase>>> GetReservationsToManage(string userId);

    // Operacje na rezerwacjach stolików
    Task<Result<TableReservation>> GetTableReservationByIdAsync(int reservationId);
    Task<Result<TableReservation>> CreateTableReservationAsync(TableReservationDto tableReservationDto);
    Task<Result> UpdateTableReservationAsync(int reservationId, TableReservationDto tableReservationDto);
    Task<Result> DeleteTableReservationAsync(int reservationId);

  
    Task<Result<IEnumerable<ReservationBase>>> GetReservationsByTableIdAsync(int tableId);

    // Metody pomocnicze
    Task<bool> IsTableAvailableAsync(int tableId, DateTime date, TimeOnly startTime, TimeOnly endTime,
        int? excludeReservationId = null);
    Task<Result> UpdateReservationStatusAsync(int reservationId, ReservationStatus status);


    Task<Result<IEnumerable<ReservationBase>>> SearchReservationsAsync(ReservationSearchParameters searchParams);
}