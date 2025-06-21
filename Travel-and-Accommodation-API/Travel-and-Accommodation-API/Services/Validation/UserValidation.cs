using FluentValidation;
using Travel_and_Accommodation_API.DataAccess.Repositories.IRepository;
using Travel_and_Accommodation_API.Models;

namespace Travel_and_Accommodation_API.Services.Validation
{
    public class UserValidation : AbstractValidator<User>
    {
        private readonly IRepository<Country> _countryRepository;

        public const int MaxNameLength = 100;

        public UserValidation(IRepository<Country> countryRepository)
        {
            _countryRepository = countryRepository ?? throw new ArgumentNullException(nameof(countryRepository));

            // First Name
            RuleFor(user => user.FirstName)
                .NotEmpty().WithMessage("First Name is required")
                .MaximumLength(MaxNameLength).WithMessage($"First Name must not exceed {MaxNameLength} characters");

            // Last Name
            RuleFor(user => user.LastName)
                .NotEmpty().WithMessage("Last Name is required")
                .MaximumLength(MaxNameLength).WithMessage($"Last Name must not exceed {MaxNameLength} characters");

            // Country
            RuleFor(user => user.CountryId)
                .NotEmpty().WithMessage("Country is required")
                .MustAsync(async (countryId, cancellation) => await CountryExistsAsync(countryId))
                .WithMessage(countryId => $"Country with ID {countryId} not found");

            // Registration Date
            RuleFor(user => user.RegistrationDate)
                .NotEmpty().WithMessage("User registration date is required");

            // Active
            RuleFor(user => user.Active)
                .NotEmpty().WithMessage("Active boolean is required");

            //Email
            RuleFor(user => user.Email)
                .EmailAddress().WithMessage("Invalid user name");
        }

        private async Task<bool> CountryExistsAsync(Guid countryId)
        {

            var country = await _countryRepository.GetByIdAsync(countryId);
            return country != null;

        }
    }
}
