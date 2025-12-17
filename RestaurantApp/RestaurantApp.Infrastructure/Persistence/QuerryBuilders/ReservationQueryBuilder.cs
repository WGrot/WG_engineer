using Microsoft.EntityFrameworkCore;
using RestaurantApp.Application.Mappers.EnumMappers;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.SearchParameters;

namespace RestaurantApp.Infrastructure.Persistence.QuerryBuilders;

public static class ReservationQueryBuilder
{
    public static IQueryable<ReservationBase>? BuildQuery(
        ApplicationDbContext context,
        ReservationSearchParameters searchParams,
        out string? errorMessage)
    {
        errorMessage = null;
        
        if (!ValidateDateRange(searchParams, out errorMessage))
        {
            return null;
        }
        NormalizeDates(searchParams);
        
        var query = context.Reservations
            .Include(r => r.Restaurant)
            .AsQueryable();
        
        query = ApplyFilters(query, searchParams);
        
        query = ApplySorting(query, searchParams);

        return query;
    }

    private static bool ValidateDateRange(ReservationSearchParameters searchParams, out string? errorMessage)
    {
        errorMessage = null;

        if (searchParams.ReservationDateFrom.HasValue &&
            searchParams.ReservationDateTo.HasValue &&
            searchParams.ReservationDateFrom > searchParams.ReservationDateTo)
        {
            errorMessage = "Invalid date range: 'reservationDateFrom' cannot be later than 'reservationDateTo'";
            return false;
        }

        return true;
    }

    private static void NormalizeDates(ReservationSearchParameters searchParams)
    {
        if (searchParams.ReservationDate.HasValue)
        {
            searchParams.ReservationDate =
                DateTime.SpecifyKind(searchParams.ReservationDate.Value, DateTimeKind.Utc);
        }

        if (searchParams.ReservationDateFrom.HasValue)
        {
            searchParams.ReservationDateFrom =
                DateTime.SpecifyKind(searchParams.ReservationDateFrom.Value, DateTimeKind.Utc);
        }

        if (searchParams.ReservationDateTo.HasValue)
        {
            searchParams.ReservationDateTo =
                DateTime.SpecifyKind(searchParams.ReservationDateTo.Value, DateTimeKind.Utc);
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

        if (searchParams.RestaurantIds != null && searchParams.RestaurantIds.Any())
        {
            query = query.Where(r => searchParams.RestaurantIds.Contains(r.RestaurantId));
        }

        if (!string.IsNullOrWhiteSpace(searchParams.RestaurantName))
        {
            var lowerName = searchParams.RestaurantName.ToLower();
            query = query.Where(r => r.Restaurant.Name.ToLower().Contains(lowerName));
        }

        if (!string.IsNullOrWhiteSpace(searchParams.UserId))
        {
            query = query.Where(r => r.UserId == searchParams.UserId);
        }
        
        if (searchParams.Status.HasValue)
        {
            query = query.Where(r => r.Status == searchParams.Status.Value.ToDomain());
        }
        
        if (!string.IsNullOrWhiteSpace(searchParams.CustomerName))
        {
            var lowerName = searchParams.CustomerName.ToLower();
            query = query.Where(r => r.CustomerName.ToLower().Contains(lowerName));
        }

        if (!string.IsNullOrWhiteSpace(searchParams.CustomerEmail))
        {
            var lowerEmail = searchParams.CustomerEmail.ToLower();
            query = query.Where(r => r.CustomerEmail.ToLower().Contains(lowerEmail));
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
            var lowerNotes = searchParams.Notes.ToLower();
            query = query.Where(r => r.Notes != null && r.Notes.ToLower().Contains(lowerNotes));
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
                .Where(r => (r.ReservationDate == now.Date && r.StartTime >= currentTime))
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