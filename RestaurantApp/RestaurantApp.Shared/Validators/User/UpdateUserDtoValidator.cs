using FluentValidation;
using RestaurantApp.Shared.DTOs.Users;

namespace RestaurantApp.Shared.Validators.User;

public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.Email)
            .EmailAddress()
            .WithMessage("Invalid email format.")
            .MaximumLength(255)
            .WithMessage("Email cannot exceed 255 characters.")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.FirstName)
            .MaximumLength(100)
            .WithMessage("First name cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.FirstName));

        RuleFor(x => x.LastName)
            .MaximumLength(100)
            .WithMessage("Last name cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.LastName));

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20)
            .WithMessage("Phone number cannot exceed 20 characters.")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}