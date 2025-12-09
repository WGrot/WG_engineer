using FluentValidation;
using RestaurantApp.Shared.DTOs.Restaurant;

namespace RestaurantApp.Shared.Validators.Resaturant;

public class CreateRestaurantDtoValidator : AbstractValidator<CreateRestaurantDto>
{
    public CreateRestaurantDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Restaurant name is required.")
            .MaximumLength(200)
            .WithMessage("Restaurant name cannot exceed 200 characters.");

        When(x => x.StructuresAddress != null, () =>
        {
            RuleFor(x => x.StructuresAddress!.City)
                .NotEmpty()
                .WithMessage("City is required when providing structured address.")
                .MaximumLength(100)
                .WithMessage("City cannot exceed 100 characters.");

            RuleFor(x => x.StructuresAddress!.Street)
                .NotEmpty()
                .WithMessage("Street is required when providing structured address.")
                .MaximumLength(200)
                .WithMessage("Street cannot exceed 200 characters.");

            RuleFor(x => x.StructuresAddress!.PostalCode)
                .NotEmpty()
                .WithMessage("Postal code is required when providing structured address.")
                .MaximumLength(20)
                .WithMessage("Postal code cannot exceed 20 characters.");

            RuleFor(x => x.StructuresAddress!.Country)
                .MaximumLength(100)
                .WithMessage("Country cannot exceed 100 characters.");
        });
    }
}