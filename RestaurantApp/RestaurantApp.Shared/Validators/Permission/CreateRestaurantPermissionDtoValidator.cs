using FluentValidation;
using RestaurantApp.Shared.DTOs.Permissions;

namespace RestaurantApp.Shared.Validators.Permission;

public class CreateRestaurantPermissionDtoValidator : AbstractValidator<CreateRestaurantPermissionDto>
{
    public CreateRestaurantPermissionDtoValidator()
    {
        RuleFor(x => x.RestaurantEmployeeId)
            .GreaterThan(0)
            .WithMessage("Employee ID must be greater than 0.");

        RuleFor(x => x.Permission)
            .IsInEnum()
            .WithMessage("Invalid permission type.");
    }
}