using RestaurantApp.Shared.DTOs.SearchParameters;

namespace RestaurantApp.Blazor.Helpers;

public static class ReservationSearchParamsHelper
{
    public static string BuildQueryString(this ReservationSearchParameters parameters)
    {
        var queryParams = new List<string>();

        queryParams.Add($"page={parameters.Page}");
        queryParams.Add($"pageSize={parameters.PageSize}");
        queryParams.Add($"sortBy={parameters.SortBy}");
        
        if (parameters.RestaurantId.HasValue)
            queryParams.Add($"restaurantId={parameters.RestaurantId}");
        
        if (!string.IsNullOrWhiteSpace(parameters.RestaurantName))
            queryParams.Add($"restaurantName={Uri.EscapeDataString(parameters.RestaurantName)}");

        if (!string.IsNullOrWhiteSpace(parameters.UserId))
            queryParams.Add($"userId={Uri.EscapeDataString(parameters.UserId)}");

        if (parameters.Status.HasValue)
            queryParams.Add($"status={parameters.Status}");

        if (!string.IsNullOrWhiteSpace(parameters.CustomerName))
            queryParams.Add($"customerName={Uri.EscapeDataString(parameters.CustomerName)}");

        if (!string.IsNullOrWhiteSpace(parameters.CustomerEmail))
            queryParams.Add($"customerEmail={Uri.EscapeDataString(parameters.CustomerEmail)}");

        if (!string.IsNullOrWhiteSpace(parameters.CustomerPhone))
            queryParams.Add($"customerPhone={Uri.EscapeDataString(parameters.CustomerPhone)}");

        if (parameters.ReservationDate.HasValue)
            queryParams.Add($"reservationDate={parameters.ReservationDate.Value:yyyy-MM-dd}");

        if (parameters.ReservationDateFrom.HasValue)
            queryParams.Add($"reservationDateFrom={parameters.ReservationDateFrom.Value:yyyy-MM-dd}");

        if (parameters.ReservationDateTo.HasValue)
            queryParams.Add($"reservationDateTo={parameters.ReservationDateTo.Value:yyyy-MM-dd}");

        if (!string.IsNullOrWhiteSpace(parameters.Notes))
            queryParams.Add($"notes={Uri.EscapeDataString(parameters.Notes)}");



        return queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
    }
}