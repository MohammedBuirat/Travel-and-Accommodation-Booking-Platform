using FluentValidation;
using Travel_and_Accommodation_API.DataAccess.Repositories.IRepository;
using Travel_and_Accommodation_API.Models;

namespace Travel_and_Accommodation_API.Services.Validation
{
    public class CityValidation : AbstractValidator<City>
    {
        private readonly IRepository<Country> _countryRepository;
        public const int MaxCityNameLength = 100;
        public const int MaxPostOfficeLength = 30;
        public const int MaxCountryLength = 100;

        public CityValidation(IRepository<Country> countryRepository)
        {
            _countryRepository = countryRepository ?? throw new ArgumentNullException(nameof(countryRepository));

            // City Name
            RuleFor(city => city.Name)
                .NotEmpty().WithMessage("City name is required")
                .MaximumLength(MaxCityNameLength).WithMessage($"City name must not exceed {MaxCityNameLength} characters");

            // Post Office
            RuleFor(city => city.PostOffice)
                .NotEmpty().WithMessage("Post office is required")
                .MaximumLength(MaxPostOfficeLength).WithMessage($"Post office must not exceed {MaxPostOfficeLength} characters");

            // Image
            RuleFor(city => city.Image)
                .NotEmpty().WithMessage("Image path is required");

            // Country
            RuleFor(city => city.CountryId)
                .NotEmpty().WithMessage("Country is required")
                .MustAsync(async (countryId, cancellation) => await CountryExistsAsync(countryId))
                .WithMessage(countryId => $"Country with ID {countryId} not found");

        }
        private async Task<bool> CountryExistsAsync(Guid countryId)
        {
            var country = await _countryRepository.GetByIdAsync(countryId);
            return country != null;
        }
    }
}
