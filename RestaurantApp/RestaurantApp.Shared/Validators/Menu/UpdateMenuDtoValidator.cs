using FluentValidation;
using RestaurantApp.Shared.DTOs.Menu;

namespace RestaurantApp.Shared.Validators.Menu;

public class UpdateMenuDtoValidator : AbstractValidator<UpdateMenuDto>
{
    public UpdateMenuDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Menu name is required.")
            .MaximumLength(100)
            .WithMessage("Menu name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}