using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Shared.Common;

namespace RestaurantApp.Api.Common;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result, ControllerBase controller)
    {
        if (result.IsSuccess)
        {
            return controller.StatusCode(result.StatusCode, result.Value);
        }
        
        var errorResponse = new { error = result.Error };
        return controller.StatusCode(result.StatusCode, errorResponse);
    }
    
    public static IActionResult ToActionResult(this Result result, ControllerBase controller)
    {
        if (result.IsSuccess)
        {
            return controller.StatusCode(result.StatusCode);
        }
        
        var errorResponse = new { error = result.Error };
        return controller.StatusCode(result.StatusCode, errorResponse);
    }
}