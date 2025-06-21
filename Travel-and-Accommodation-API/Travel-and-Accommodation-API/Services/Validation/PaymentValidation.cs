using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Travel_and_Accommodation_API.DataAccess.Repositories.IRepository;
using Travel_and_Accommodation_API.Models;

namespace Travel_and_Accommodation_API.Services.Validation
{
    public class PaymentValidation : AbstractValidator<Payment>
    {
        private readonly UserManager<User> _userManager;
        private readonly IRepository<Booking> _bookingRepository;

        public PaymentValidation(UserManager<User> userManager,
            IRepository<Booking> bookingRepository)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));

            // User ID
            RuleFor(payment => payment.UserId)
                .NotEmpty().WithMessage("User ID is required")
                .MustAsync(async (userId, cancellation) => await UserExistsAsync(userId))
                .WithMessage("Invalid user ID");

            // Booking ID
            RuleFor(payment => payment.BookingId)
                .NotEmpty().WithMessage("Booking ID is required")
                .MustAsync(async (bookingId, cancellation) => await BookingExistsAsync(bookingId))
                .WithMessage("Invalid booking ID");

            // Payment Method
            RuleFor(payment => payment.Method)
                .NotNull().WithMessage("Payment method is required")
                .IsInEnum().WithMessage("Invalid payment method");

            // Amount
            RuleFor(payment => payment.Amount)
                .NotNull().WithMessage("Amount is required")
                .GreaterThanOrEqualTo(0).WithMessage("Amount must be greater than or equal to 0");
        }

        private async Task<bool> UserExistsAsync(string userId)
        {
            var userExists = (await _userManager.FindByIdAsync(userId) != null);
            return userExists;
        }

        private async Task<bool> BookingExistsAsync(Guid bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            return booking != null;
        }
    }
}
