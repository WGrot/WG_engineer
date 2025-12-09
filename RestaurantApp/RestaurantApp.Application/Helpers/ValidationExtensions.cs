using FluentValidation.Results;
using RestaurantApp.Shared.Common;

namespace RestaurantApp.Application.Helpers;

public static class ValidationExtensions
{
    public static Result ToResult(this ValidationResult validationResult)
    {
        if (validationResult.IsValid)
            return Result.Success();

        var errors = validationResult.Errors
            .Select(e => e.ErrorMessage)
            .ToList();

        return Result.ValidationError(errors);
    }

    public static Result<T> ToResult<T>(this ValidationResult validationResult)
    {
        if (validationResult.IsValid)
            throw new InvalidOperationException("Cannot convert valid result without value.");

        var errors = validationResult.Errors
            .Select(e => e.ErrorMessage)
            .ToList();

        return Result<T>.ValidationError(errors);
    }
}