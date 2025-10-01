namespace RestaurantApp.Api.Functional;

public record ApiError(string Message, int StatusCode)
{
    public static ApiError BadRequest(string message) => new(message, StatusCodes.Status400BadRequest);
    public static ApiError NotFound(string message) => new(message, StatusCodes.Status404NotFound);
    public static ApiError Conflict(string message) => new(message, StatusCodes.Status409Conflict);
    public static ApiError Internal(string message) => new(message, StatusCodes.Status500InternalServerError);
}