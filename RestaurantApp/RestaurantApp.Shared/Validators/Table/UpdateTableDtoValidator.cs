using FluentValidation;
using RestaurantApp.Shared.DTOs.Tables;

namespace RestaurantApp.Shared.Validators.Table;

public class UpdateTableDtoValidator : AbstractValidator<UpdateTableDto>
{
    public UpdateTableDtoValidator()
    {
        RuleFor(x => x.TableNumber)
            .NotEmpty()
            .WithMessage("Table number is required.")
            .MaximumLength(20)
            .WithMessage("Table number cannot exceed 20 characters.");

        RuleFor(x => x.Capacity)
            .GreaterThan(0)
            .WithMessage("Capacity must be greater than 0.")
            .LessThanOrEqualTo(100)
            .WithMessage("Capacity cannot exceed 50.");

        RuleFor(x => x.Location)
            .MaximumLength(100)
            .WithMessage("Location cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Location));
    }
}