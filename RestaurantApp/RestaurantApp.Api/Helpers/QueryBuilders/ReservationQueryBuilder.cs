using Microsoft.EntityFrameworkCore;
using RestaurantApp.Domain.Models;
using RestaurantApp.Infrastructure.Persistence;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.SearchParameters;

namespace RestaurantApp.Api.Helpers.QueryBuilders;

public static class ReservationQueryBuilder
{
    public static Result<IQueryable<ReservationBase>> BuildQuery(ApplicationDbContext _context,
        ReservationSearchParameters searchParams)
    {
        // --- Walidacja ---
        if (searchParams.ReservationDate.HasValue &&
            searchParams.ReservationDateTo.HasValue &&
            searchParams.ReservationDateFrom > searchParams.ReservationDateTo)
        {
            return Result<IQueryable<ReservationBase>>.Failure(
                "Invalid date range: 'reservationDateFrom' cannot be later than 'reservationDateTo'", 400);
        }

        // --- Normalizacja dat ---
        NormalizeDates(searchParams);

        // --- Budowanie zapytania ---
        var query = _context.Reservations
            .Include(r => r.Restaurant)
            .AsQueryable();

        // --- Filtrowanie ---
        query = ApplyFilters(query, searchParams);
        
        // --- Sortowanie ---
        query = ApplySorting(query, searchParams);

        return Result<IQueryable<ReservationBase>>.Success(query);
    }

    private static void NormalizeDates(ReservationSearchParameters searchParams)
    {
        if (searchParams.ReservationDate.HasValue)
        {
            searchParams.ReservationDate =
                DateTime.SpecifyKind((DateTime)searchParams.ReservationDate, DateTimeKind.Utc);
        }

        if (searchParams.ReservationDateFrom.HasValue)
        {
            searchParams.ReservationDateFrom =
                DateTime.SpecifyKind((DateTime)searchParams.ReservationDateFrom, DateTimeKind.Utc);
        }

        if (searchParams.ReservationDateTo.HasValue)
        {
            searchParams.ReservationDateTo =
                DateTime.SpecifyKind((DateTime)searchParams.ReservationDateTo, DateTimeKind.Utc);
        }
    }

    private static IQueryable<ReservationBase> ApplyFilters(
        IQueryable<ReservationBase> query, 
        ReservationSearchParameters searchParams)
    {
        if (searchParams.RestaurantId.HasValue)
        {
            query = query.Where(r => r.RestaurantId == searchParams.RestaurantId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchParams.RestaurantName))
        {
            query = query.Where(r => r.Restaurant.Name.ToLower().Contains(searchParams.RestaurantName.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(searchParams.UserId))
        {
            query = query.Where(r => r.UserId == searchParams.UserId);
        }

        if (searchParams.Status.HasValue)
        {
            query = query.Where(r => r.Status == searchParams.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchParams.CustomerName))
        {
            query = query.Where(r => r.CustomerName.ToLower().Contains(searchParams.CustomerName.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(searchParams.CustomerEmail))
        {
            query = query.Where(r => r.CustomerEmail.ToLower().Contains(searchParams.CustomerEmail.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(searchParams.CustomerPhone))
        {
            var normalizedPhone = searchParams.CustomerPhone.Replace(" ", "").Replace("-", "");
            query = query.Where(r => r.CustomerPhone.Replace(" ", "").Replace("-", "").Contains(normalizedPhone));
        }

        if (searchParams.ReservationDate.HasValue)
        {
            var dateOnly = searchParams.ReservationDate.Value.Date;
            query = query.Where(r => r.ReservationDate.Date == dateOnly);
        }

        if (searchParams.ReservationDateFrom.HasValue)
        {
            query = query.Where(r => r.ReservationDate >= searchParams.ReservationDateFrom.Value);
        }

        if (searchParams.ReservationDateTo.HasValue)
        {
            query = query.Where(r => r.ReservationDate <= searchParams.ReservationDateTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchParams.Notes))
        {
            query = query.Where(r => r.Notes != null && r.Notes.ToLower().Contains(searchParams.Notes.ToLower()));
        }

        return query;
    }

    private static IQueryable<ReservationBase> ApplySorting(
        IQueryable<ReservationBase> query, 
        ReservationSearchParameters searchParams)
    {
        var now = DateTime.UtcNow;
        var currentTime = TimeOnly.FromDateTime(now);

        return searchParams.SortBy?.ToLower() switch
        {
            "oldest" => query
                .OrderBy(r => r.CreatedAt)
                .ThenBy(r => r.StartTime),

            "next" => query
                .Where(r => r.ReservationDate == now.Date && r.StartTime >= currentTime
                            || (r.ReservationDate > now.Date))
                .OrderBy(r => r.ReservationDate)
                .ThenBy(r => r.StartTime),

            "newest" => query
                .OrderByDescending(r => r.CreatedAt)
                .ThenByDescending(r => r.StartTime),

            _ => query
                .OrderByDescending(r => r.ReservationDate)
                .ThenByDescending(r => r.StartTime)
        };
    }
}