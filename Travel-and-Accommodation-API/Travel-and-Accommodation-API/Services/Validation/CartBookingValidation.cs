using FluentValidation;
using Travel_and_Accommodation_API.Models;
using Microsoft.AspNetCore.Identity;

namespace Travel_and_Accommodation_API.Services.Validation
{
    public class CartBookingValidation : AbstractValidator<CartBooking>
    {
        private readonly UserManager<User> _userManager;

        public const int MaxSpecialRequestsLength = 1000;

        public CartBookingValidation(UserManager<User> userManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));

            // User ID
            RuleFor(cartBooking => cartBooking.UserId)
                .NotEmpty().WithMessage("User ID is required")
                .MustAsync(async (userId, cancellation) => await UserExistsAsync(userId))
                .WithMessage(userId => $"User with ID {userId} not found");

            // Check-in Date
            RuleFor(cartBooking => cartBooking.CheckInDate)
                .NotEmpty().WithMessage("Check-in date is required");

            // Check-out Date
            RuleFor(cartBooking => cartBooking.CheckOutDate)
                .NotEmpty().WithMessage("Check-out date is required")
                .GreaterThan(cartBooking => cartBooking.CheckInDate).WithMessage("Check-out date must be after check-in date");

            // Special Requests
            RuleFor(cartBooking => cartBooking.SpecialRequests)
                .MaximumLength(MaxSpecialRequestsLength).WithMessage($"Special requests must not exceed {MaxSpecialRequestsLength} characters")
                .When(cartBooking => !string.IsNullOrEmpty(cartBooking.SpecialRequests));

            // Number of Adults
            RuleFor(cartBooking => cartBooking.NumOfAdults)
                .NotNull().WithMessage("Number of adults is required")
                .GreaterThanOrEqualTo(0).WithMessage("Number of adults must be greater than or equal to 0");

            // Number of Children
            RuleFor(cartBooking => cartBooking.NumOfChildren)
                .NotNull().WithMessage("Number of children is required")
                .GreaterThanOrEqualTo(0).WithMessage("Number of children must be greater than or equal to 0");
        }

        private async Task<bool> UserExistsAsync(string userId)
        {
            var userExists = (await _userManager.FindByIdAsync(userId) != null);
            return userExists;
        }
    }
}
