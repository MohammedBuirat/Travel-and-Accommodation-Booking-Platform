using FluentValidation;
using Travel_and_Accommodation_API.Models;

namespace Travel_and_Accommodation_API.Services.Validation
{
    public class CountryValidation : AbstractValidator<Country>
    {
        public const int MaxNameLength = 100;
        public const int MaxCurrencyLength = 100;
        public const int MaxLanguageLength = 30;
        public const int MaxCountryCodeLength = 10;
        public const int MaxTimeZoneLength = 30;

        public CountryValidation()
        {
            // Name
            RuleFor(country => country.Name)
                .NotEmpty().WithMessage("Country name is required")
                .MaximumLength(MaxNameLength).WithMessage($"Country name must not exceed {MaxNameLength} characters");

            // Currency
            RuleFor(country => country.Currency)
                .NotEmpty().WithMessage("Currency is required")
                .MaximumLength(MaxCurrencyLength).WithMessage($"Currency must not exceed {MaxCurrencyLength} characters");

            // Language
            RuleFor(country => country.Language)
                .NotEmpty().WithMessage("Language is required")
                .MaximumLength(MaxLanguageLength).WithMessage($"Language must not exceed {MaxLanguageLength} characters");

            // Country Code
            RuleFor(country => country.CountryCode)
                .NotEmpty().WithMessage("Country code is required")
                .MaximumLength(MaxCountryCodeLength).WithMessage($"Country code must not exceed {MaxCountryCodeLength} characters");

            // TimeZone
            RuleFor(country => country.TimeZone)
                .NotEmpty().WithMessage("Time zone is required")
                .MaximumLength(MaxTimeZoneLength).WithMessage($"Time zone must not exceed {MaxTimeZoneLength} characters");

            // Flag Image
            RuleFor(country => country.FlagImage)
                .NotEmpty().WithMessage("Flag image path is required");
        }
    }
}
