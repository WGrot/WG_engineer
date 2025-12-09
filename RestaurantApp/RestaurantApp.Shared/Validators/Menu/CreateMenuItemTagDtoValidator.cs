using FluentValidation;
using RestaurantApp.Shared.DTOs.Menu.Tags;

namespace RestaurantApp.Shared.Validators.Menu;

public class CreateMenuItemTagDtoValidator : AbstractValidator<CreateMenuItemTagDto>
{
    public CreateMenuItemTagDtoValidator()
    {
        RuleFor(x => x.RestaurantId)
            .GreaterThan(0)
            .WithMessage("Restaurant ID must be greater than 0.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Tag name is required.")
            .MaximumLength(50)
            .WithMessage("Tag name cannot exceed 50 characters.");

        RuleFor(x => x.ColorHex)
            .NotEmpty()
            .WithMessage("Color is required.")
            .Matches(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")
            .WithMessage("Color must be a valid hex color (e.g. #FFFFFF or #FFF).");
    }
}