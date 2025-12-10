using FluentValidation;
using RestaurantApp.Shared.DTOs.Menu.Variants;

namespace RestaurantApp.Shared.Validators.Menu;

public class MenuItemVariantDtoValidator : AbstractValidator<MenuItemVariantDto>
{
    public MenuItemVariantDtoValidator()
    {
        RuleFor(x => x.MenuItemId)
            .GreaterThan(0)
            .WithMessage("MenuItem ID must be greater than 0.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Variant name is required.")
            .MaximumLength(100)
            .WithMessage("Variant name cannot exceed 100 characters.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Price cannot be negative.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}