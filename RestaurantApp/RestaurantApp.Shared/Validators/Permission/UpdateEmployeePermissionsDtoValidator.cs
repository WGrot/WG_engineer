using FluentValidation;
using RestaurantApp.Shared.DTOs.Permissions;

namespace RestaurantApp.Shared.Validators.Permission;

public class UpdateEmployeePermissionsDtoValidator : AbstractValidator<UpdateEmployeePermisionsDto>
{
    public UpdateEmployeePermissionsDtoValidator()
    {
        RuleFor(x => x.EmployeeId)
            .GreaterThan(0)
            .WithMessage("Employee ID must be greater than 0.");

        RuleFor(x => x.Permissions)
            .NotNull()
            .WithMessage("Permissions list cannot be null.");

        RuleForEach(x => x.Permissions)
            .IsInEnum()
            .WithMessage($"Invalid permission type in list.");
    }
}