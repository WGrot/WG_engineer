using FluentValidation;
using RestaurantApp.Shared.DTOs.Review;

namespace RestaurantApp.Shared.Validators.Review;

public class UpdateReviewDtoValidator : AbstractValidator<UpdateReviewDto>
{
    public UpdateReviewDtoValidator()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Review rating must be between 1 and 5.");

        RuleFor(x => x.Content)
            .MaximumLength(2000)
            .WithMessage("Comment cannot exceed 2000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Content));
    }
}