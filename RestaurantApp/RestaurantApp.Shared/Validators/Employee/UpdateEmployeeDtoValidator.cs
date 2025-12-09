using FluentValidation;
using RestaurantApp.Shared.DTOs.Employees;

namespace RestaurantApp.Shared.Validators.Employee;


public class UpdateEmployeeDtoValidator : AbstractValidator<UpdateEmployeeDto>
{
    public UpdateEmployeeDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("ID must be greater than 0.");

        RuleFor(x => x.RoleEnumDto)
            .IsInEnum().WithMessage("Invalid role.");
    }
}