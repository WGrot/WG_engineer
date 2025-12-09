using FluentValidation;
using RestaurantApp.Shared.DTOs.Permissions;

namespace RestaurantApp.Shared.Validators.Permission;

public class RestaurantPermissionDtoValidator : AbstractValidator<RestaurantPermissionDto>
{
    public RestaurantPermissionDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Permission ID must be greater than 0.");

        RuleFor(x => x.RestaurantEmployeeId)
            .GreaterThan(0)
            .WithMessage("Employee ID must be greater than 0.");

        RuleFor(x => x.Permission)
            .IsInEnum()
            .WithMessage("Invalid permission type.");
    }
}