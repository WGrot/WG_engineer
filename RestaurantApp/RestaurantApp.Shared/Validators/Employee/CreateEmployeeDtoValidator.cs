using FluentValidation;
using RestaurantApp.Shared.DTOs.Employees;

namespace RestaurantApp.Shared.Validators.Employee;

public class CreateEmployeeDtoValidator : AbstractValidator<CreateEmployeeDto>
{
    public CreateEmployeeDtoValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.RestaurantId)
            .GreaterThan(0).WithMessage("Restaurant ID must be greater than 0.");

        RuleFor(x => x.RoleEnumDto)
            .IsInEnum().WithMessage("Invalid role.");
    }
}