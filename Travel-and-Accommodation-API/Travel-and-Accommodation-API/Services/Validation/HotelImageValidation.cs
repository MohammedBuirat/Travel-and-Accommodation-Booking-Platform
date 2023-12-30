using FluentValidation;
using Travel_and_Accommodation_API.DataAccess.Repositories.IRepository;
using Travel_and_Accommodation_API.Models;

namespace Travel_and_Accommodation_API.Services.Validation
{
    public class HotelImageValidation : AbstractValidator<HotelImage>
    {
        private readonly IRepository<Hotel> _hotelRepository;

        public HotelImageValidation(IRepository<Hotel> hotelRepository)
        {
            _hotelRepository = hotelRepository ?? throw new ArgumentNullException(nameof(hotelRepository));

            RuleFor(image => image.ImageString)
                .NotEmpty().WithMessage("Image URL is required");

            // Hotel ID
            RuleFor(image => image.HotelId)
                .NotEmpty().WithMessage("Hotel ID is required")
                .MustAsync(async (hotelId, cancellation) => await HotelExistsAsync(hotelId))
                .WithMessage(hotelId => $"Hotel with ID {hotelId} not found");
        }

        private async Task<bool> HotelExistsAsync(Guid hotelId)
        {

            var hotel = await _hotelRepository.GetByIdAsync(hotelId);
            return hotel != null;

        }
    }
}
