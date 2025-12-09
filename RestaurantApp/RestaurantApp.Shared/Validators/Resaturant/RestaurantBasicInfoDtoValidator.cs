using FluentValidation;
using RestaurantApp.Shared.DTOs.Restaurant;

namespace RestaurantApp.Shared.Validators.Resaturant;

public class RestaurantBasicInfoDtoValidator : AbstractValidator<RestaurantBasicInfoDto>
{
    public RestaurantBasicInfoDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Restaurant name is required.")
            .MaximumLength(200)
            .WithMessage("Restaurant name cannot exceed 200 characters.");

        RuleFor(x => x.Address)
            .NotEmpty()
            .WithMessage("Address is required.")
            .MaximumLength(500)
            .WithMessage("Address cannot exceed 500 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage("Description cannot exceed 2000 characters.")
            .When(x => x.Description != null);
    }
}