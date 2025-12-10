using FluentValidation;
using RestaurantApp.Shared.DTOs.Reservation;

namespace RestaurantApp.Shared.Validators.Reservation;

public class CreateTableReservationDtoValidator : AbstractValidator<CreateTableReservationDto>
{
    public CreateTableReservationDtoValidator()
    {
        RuleFor(x => x.RestaurantId)
            .GreaterThan(0)
            .WithMessage("Restaurant ID must be greater than 0.");

        RuleFor(x => x.TableId)
            .GreaterThan(0)
            .WithMessage("Table ID must be greater than 0.");

        RuleFor(x => x.NumberOfGuests)
            .GreaterThan(0)
            .WithMessage("Number of guests must be greater than 0.")
            .LessThanOrEqualTo(50)
            .WithMessage("Number of guests cannot exceed 50.");

        RuleFor(x => x.CustomerName)
            .NotEmpty()
            .WithMessage("Customer name is required.")
            .MaximumLength(100)
            .WithMessage("Customer name cannot exceed 100 characters.");

        RuleFor(x => x.CustomerEmail)
            .NotEmpty()
            .WithMessage("Customer email is required.")
            .EmailAddress()
            .WithMessage("Invalid email format.")
            .MaximumLength(255)
            .WithMessage("Customer email cannot exceed 255 characters.");

        RuleFor(x => x.CustomerPhone)
            .MaximumLength(20)
            .WithMessage("Customer phone cannot exceed 20 characters.")
            .When(x => !string.IsNullOrEmpty(x.CustomerPhone));
        

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Notes cannot exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}