using FluentValidation;
using Travel_and_Accommodation_API.DataAccess.Repositories.IRepository;
using Travel_and_Accommodation_API.Models;

namespace Travel_and_Accommodation_API.Services.Validation
{
    public class HotelValidation : AbstractValidator<Hotel>
    {
        private readonly IRepository<City> _cityRepository;
        public const int MaxHotelNameLength = 255;
        public const int MaxDescriptionLength = 1000;
        public const int MaxCheckInOutTimeLength = 30;
        public const int MaxAddressLength = 500;

        public HotelValidation(IRepository<City> cityRepository)
        {
            _cityRepository = cityRepository ?? throw new ArgumentNullException(nameof(cityRepository));

            // Hotel Name
            RuleFor(hotel => hotel.Name)
                .NotEmpty().WithMessage("Hotel name is required")
                .MaximumLength(MaxHotelNameLength).WithMessage($"Hotel name must not exceed {MaxHotelNameLength} characters");

            // Description
            RuleFor(hotel => hotel.Description)
                .MaximumLength(MaxDescriptionLength).WithMessage($"Description must not exceed {MaxDescriptionLength} characters")
                .When(hotel => !string.IsNullOrEmpty(hotel.Description));

            // City
            RuleFor(hotel => hotel.CityId)
                .NotEmpty().WithMessage("City ID is required")
                .MustAsync(async (CityId, cancellation) => await CityExistsAsync(CityId));

            // Owner
            RuleFor(hotel => hotel.Owner)
                .NotEmpty().WithMessage("Owner name is required")
                .MaximumLength(MaxHotelNameLength).WithMessage($"Owner name must not exceed {MaxHotelNameLength} characters");

            // Check-in Time
            RuleFor(hotel => hotel.CheckInTime)
                .NotEmpty().WithMessage("Check-in time is required")
                .MaximumLength(MaxCheckInOutTimeLength).WithMessage($"Check-in time must not exceed {MaxCheckInOutTimeLength} characters");

            // Check-out Time
            RuleFor(hotel => hotel.CheckOutTime)
                .NotEmpty().WithMessage("Check-out time is required")
                .MaximumLength(MaxCheckInOutTimeLength).WithMessage($"Check-out time must not exceed {MaxCheckInOutTimeLength} characters");

            // Number of Ratings
            RuleFor(hotel => hotel.NumOfRatings)
                .GreaterThanOrEqualTo(0).WithMessage("Number of ratings must be greater than or equal to 0");

            // Address
            RuleFor(hotel => hotel.Address)
                .NotEmpty().WithMessage("Hotel address is required")
                .MaximumLength(MaxAddressLength).WithMessage($"Address must not exceed {MaxAddressLength} characters");

            // Latitude
            RuleFor(hotel => hotel.Latitude)
                .NotNull().WithMessage("Latitude is required");

            // Longitude
            RuleFor(hotel => hotel.Longitude)
                .NotNull().WithMessage("Longitude is required");

            //Distance From City Center
            RuleFor(hotel => hotel.DistanceFromCityCenter)
                .NotNull().WithMessage("Hotel Distance from city center is required")
                .GreaterThanOrEqualTo(0).WithMessage("Distance from city center can't be negative");
        }

        private async Task<bool> CityExistsAsync(Guid cityId)
        {
                var city = await _cityRepository.GetByIdAsync(cityId);
                return city != null;
        }
    }
}
