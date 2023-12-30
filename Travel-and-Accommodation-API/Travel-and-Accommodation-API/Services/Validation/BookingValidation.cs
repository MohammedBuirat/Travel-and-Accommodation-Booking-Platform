using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Travel_and_Accommodation_API.DataAccess.Repositories.IRepository;
using Travel_and_Accommodation_API.Models;

namespace Travel_and_Accommodation_API.Services.Validation
{
    public class BookingValidation : AbstractValidator<Booking>
    {
        private readonly UserManager<User> _userManager;
        private readonly IRepository<Hotel> _hotelRepository;

        public const int MaxSpecialRequestsLength = 1000;

        public BookingValidation(UserManager<User> userManager,
            IRepository<Hotel> hotelRepository)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _hotelRepository = hotelRepository ?? throw new ArgumentNullException(nameof(hotelRepository));

            // User ID
            RuleFor(booking => booking.UserId)
                .NotEmpty().WithMessage("User ID is required")
                .MustAsync(async (userId, cancellation) => await UserExistsAsync(userId))
                .WithMessage(userId => $"User with ID {userId} not found");

            // Hotel ID
            RuleFor(booking => booking.HotelId)
                .NotEmpty().WithMessage("Hotel ID is required")
                .MustAsync(async (hotelId, cancellation) => await HotelExistsAsync(hotelId))
                .WithMessage(hotelId => $"Hotel with ID {hotelId} not found");

            // Check-in Date
            RuleFor(booking => booking.CheckInDate)
                .NotEmpty().WithMessage("Check-in date is required");

            // Check-out Date
            RuleFor(booking => booking.CheckOutDate)
                .NotEmpty().WithMessage("Check-out date is required")
                .GreaterThan(booking => booking.CheckInDate).WithMessage("Check-out date must be after check-in date");

            // Total Price
            RuleFor(booking => booking.TotalPrice)
                .NotNull().WithMessage("Total price is required");

            // Special Requests
            RuleFor(booking => booking.SpecialRequests)
                .MaximumLength(MaxSpecialRequestsLength).WithMessage($"Special requests must not exceed {MaxSpecialRequestsLength} characters")
                .When(booking => !string.IsNullOrEmpty(booking.SpecialRequests));

            // Paid
            RuleFor(booking => booking.Paid)
                .NotNull().WithMessage("Payment status is required");

            // Number of Adults
            RuleFor(booking => booking.NumOfAdults)
                .NotNull().WithMessage("Number of adults is required")
                .GreaterThanOrEqualTo(0).WithMessage("Number of adults must be greater than or equal to 0");

            // Number of Children
            RuleFor(booking => booking.NumOfChildren)
                .NotNull().WithMessage("Number of children is required")
                .GreaterThanOrEqualTo(0).WithMessage("Number of children must be greater than or equal to 0");
        }

        private async Task<bool> UserExistsAsync(string userId)
        {
            var userExists = (await _userManager.FindByIdAsync(userId) != null);
            return userExists;
        }

        private async Task<bool> HotelExistsAsync(Guid hotelId)
        {
            var hotel = await _hotelRepository.GetByIdAsync(hotelId);
            return hotel != null;
        }
    }
}
