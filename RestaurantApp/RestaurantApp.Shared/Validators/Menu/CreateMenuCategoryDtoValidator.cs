using FluentValidation;
using RestaurantApp.Shared.DTOs.Menu.Categories;

namespace RestaurantApp.Shared.Validators.Menu;

public class CreateMenuCategoryDtoValidator: AbstractValidator<CreateMenuCategoryDto>
{
    public CreateMenuCategoryDtoValidator()
    {
        RuleFor(x => x.MenuId)
            .GreaterThan(0)
            .WithMessage("Menu ID must be greater than 0.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Category name is required.")
            .MaximumLength(100)
            .WithMessage("Category name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Display order must be 0 or greater.");
    }
}