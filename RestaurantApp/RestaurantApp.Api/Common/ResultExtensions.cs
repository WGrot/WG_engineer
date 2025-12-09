using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Shared.Common;

namespace RestaurantApp.Api.Common;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return new ObjectResult(result.Value)
            {
                StatusCode = result.StatusCode
            };
        }

        return new ObjectResult(new { errors = result.Errors })
        {
            StatusCode = result.StatusCode
        };
    }

    public static IActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
        {
            return new StatusCodeResult(result.StatusCode);
        }

        return new ObjectResult(new { errors = result.Errors })
        {
            StatusCode = result.StatusCode
        };
    }
}