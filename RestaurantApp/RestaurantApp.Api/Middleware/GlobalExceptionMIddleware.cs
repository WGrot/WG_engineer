using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Shared.Common;

namespace RestaurantApp.Api.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken ct)
    {
        _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

        var (statusCode, result) = exception switch
        {
            DbUpdateConcurrencyException => (
                StatusCodes.Status409Conflict,
                Result.Failure("Data was modified by another user. Please refresh and try again.")),

            DbUpdateException => (
                StatusCodes.Status400BadRequest,
                Result.Failure("Database operation failed.")),

            OperationCanceledException => (
                StatusCodes.Status499ClientClosedRequest,
                Result.Failure("Request was cancelled.")),

            _ => (
                StatusCodes.Status500InternalServerError,
                Result.Failure("An unexpected error occurred."))
        };

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(result, ct);

        return true;
    }
}
