using FluentValidation;
using Travel_and_Accommodation_API.DataAccess.Repositories.IRepository;
using Travel_and_Accommodation_API.Models;

namespace Travel_and_Accommodation_API.Services.Validation
{
    public class AttractionValidation : AbstractValidator<Attraction>
    {
        private readonly IRepository<City> _repository;
        private const int MaxAttractionNameLength = 255;

        public AttractionValidation(IRepository<City> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));

            // Attraction Name
            RuleFor(attraction => attraction.Name)
                .NotEmpty().WithMessage("Attraction name is required")
                .MaximumLength(MaxAttractionNameLength).WithMessage($"Attraction name must not exceed {MaxAttractionNameLength} characters");

            // Latitude
            RuleFor(attraction => attraction.Latitude)
                .NotNull().WithMessage("Attraction Latitude is required");

            // Longitude
            RuleFor(attraction => attraction.Longitude)
                .NotNull().WithMessage("Attraction Longitude is required");

            // City
            RuleFor(attraction => attraction.CityId)
                .NotEmpty().WithMessage("City ID is required")
                .MustAsync(async (CityId, cancellation) => await CityExistsAsync(CityId));
        }

        private async Task<bool> CityExistsAsync(Guid cityId)
        {
            var city = await _repository.GetByIdAsync(cityId);
            return city != null;
        }
    }
}
