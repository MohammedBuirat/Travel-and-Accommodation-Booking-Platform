using FluentValidation;
using Travel_and_Accommodation_API.Models;
using Microsoft.AspNetCore.Identity;
using Travel_and_Accommodation_API.DataAccess.Repositories.IRepository;

namespace Travel_and_Accommodation_API.Services.Validation
{
    public class ReviewValidation : AbstractValidator<Review>
    {
        private readonly IRepository<Hotel> _hotelRepository;
        private readonly UserManager<User> _userManager;

        public const int MaxDescriptionLength = 1000;
        public const int MinRating = 0;
        public const int MaxRating = 10;

        public ReviewValidation(IRepository<Hotel> hotelRepository, UserManager<User> userManager)
        {
            _hotelRepository = hotelRepository ?? throw new ArgumentNullException(nameof(hotelRepository));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));

            // Hotel ID
            RuleFor(review => review.HotelId)
                .NotEmpty().WithMessage("Hotel ID is required")
                .MustAsync(async (hotelId, cancellation) => await HotelExistsAsync(hotelId))
                .WithMessage(hotelId => $"Hotel with ID {hotelId} not found");

            // User ID
            RuleFor(review => review.UserId)
                .NotEmpty().WithMessage("User ID is required")
                .MustAsync(async (userId, cancellation) => await UserExistsAsync(userId))
                .WithMessage(userId => $"User with ID {userId} not found");

            // Description
            RuleFor(review => review.Description)
                .MaximumLength(MaxDescriptionLength).WithMessage($"Description must not exceed {MaxDescriptionLength} characters")
                .When(review => !string.IsNullOrEmpty(review.Description));

            // Rating
            RuleFor(review => review.OverAllRating)
                .NotNull().WithMessage("Rating is required")
                .InclusiveBetween(MinRating, MaxRating).WithMessage($"Rating must be between {MinRating} and {MaxRating}");

            // Date of Review
            RuleFor(review => review.DateOfReview)
                .NotEmpty().WithMessage("Date of review is required");
        }

        private async Task<bool> HotelExistsAsync(Guid hotelId)
        {
            var hotel = await _hotelRepository.GetByIdAsync(hotelId);
            return hotel != null;
        }

        private async Task<bool> UserExistsAsync(string userId)
        {
            var userExists = (await _userManager.FindByIdAsync(userId) != null);
            return userExists;
        }
    }
}
