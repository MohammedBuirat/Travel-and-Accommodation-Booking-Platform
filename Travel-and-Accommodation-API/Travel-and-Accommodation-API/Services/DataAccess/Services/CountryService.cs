using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Travel_and_Accommodation_API.DataAccess.Repositories.IRepository;
using Travel_and_Accommodation_API.Dto.Country;
using Travel_and_Accommodation_API.Exceptions_and_logs;
using Travel_and_Accommodation_API.Helpers;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Services.DataAccess.IServices;
using Travel_and_Accommodation_API.Services.ImageService;
using Travel_and_Accommodation_API.Services.Validation;

namespace Travel_and_Accommodation_API.Services.DataAccess.Services
{
    public class CountryService : ICountryService
    {
        private readonly IRepository<Country> _countryRepository;
        private readonly ILogger<CountryService> _logger;
        private readonly IImageService _imageService;
        private readonly CountryValidation _countryValidation;
        private readonly IMapper _mapper;

        public CountryService(IRepository<Country> countryRepository,
            ILogger<CountryService> logger,
            IImageService imageService,
            IMapper mapper,
            CountryValidation countryValidation)
        {
            _countryRepository = countryRepository
                ?? throw new ArgumentNullException(nameof(countryRepository));
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            _imageService = imageService
                ?? throw new ArgumentNullException(nameof(imageService));
            _countryValidation = countryValidation
                ?? throw new ArgumentNullException(nameof(countryValidation));
        }

        public async Task<CountryDto> GetByIdAsync(Guid id)
        {
            try
            {
                var country = await GetCountry(id);
                var countryDto = _mapper.Map<CountryDto>(country);
                Logs<Country>.GetEntityLog(_logger, "Country", id);
                return countryDto;
            }
            catch (ElementNotFoundException)
            {
                return null;
            }
            catch (Exception ex)
            {
                Logs<Country>.GetEntityException(_logger, ex, "Country", id);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            try
            {
                var country = await GetCountry(id);
                await _countryRepository.DeleteAsync(id);
                if (!string.IsNullOrEmpty(country.FlagImage))
                {
                    await _imageService.DeleteFileAsync(country.FlagImage);
                }
                Logs<Country>.DeleteEntityLog(_logger, "Country", country);
            }
            catch (Exception ex)
            {
                Logs<Country>.DeleteEntityException(_logger, ex, "Country", id);
                throw;
            }
        }

        public async Task<IEnumerable<CountryDto>> GetAllAsync(int? pageSize, int? pageNumber)
        {
            try
            {
                var paging = new Paging(pageSize, pageNumber);
                var countries = await _countryRepository.GetAllAsync(paging);
                var countryDtos = _mapper.Map<List<CountryDto>>(countries);
                Logs<Country>.GetEntitiesLog(_logger, "Countries");
                return countryDtos;
            }
            catch (Exception ex)
            {
                Logs<Country>.GetEntitiesException(_logger, ex, "Countries");
                throw;
            }
        }

        public async Task UpdateAsync(Guid id, CountryToAdd entity)
        {
            try
            {
                var existingCountry = await GetCountry(id);
                var updatedCountry = existingCountry.Clone();
                _mapper.Map(entity, updatedCountry);

                await ValidateCountryAsync(updatedCountry);

                await _countryRepository.UpdateAsync(updatedCountry);
                if (entity.ImageFile != null)
                {
                    string fileName = existingCountry.FlagImage;
                    await _imageService.UpdateFile(fileName, entity.ImageFile);
                }
                Logs<Country>.UpdateEntityLog(_logger, "Country", existingCountry);
            }
            catch (Exception ex)
            {
                Logs<Country>.UpdateEntityException(_logger, ex, "Country", id);
                throw;
            }
        }

        public async Task PartialUpdateAsync(Guid id, JsonPatchDocument<Country> patchDocument)
        {
            try
            {
                var existingCountry = await GetCountry(id);
                var updatedCountry = existingCountry.Clone();
                patchDocument.ApplyTo(existingCountry);

                await ValidateCountryAsync(updatedCountry);

                await _countryRepository.UpdateAsync(updatedCountry);
                Logs<Country>.UpdateEntityLog(_logger, "Country", existingCountry);
            }
            catch (Exception ex)
            {
                Logs<Country>.UpdateEntityException(_logger, ex, "Country", id);
                throw;
            }
        }

        public async Task<CountryDto> AddAsync(CountryToAdd countryToAdd)
        {
            try
            {
                var country = _mapper.Map<Country>(countryToAdd);
                await ValidateCountryAsync(country);
                var addedCountry = await _countryRepository.AddAsync(country);
                if (countryToAdd.ImageFile != null)
                {
                    try
                    {
                        string imageString = $"country{addedCountry.Id}";
                        await _imageService.AddFileAsync(countryToAdd.ImageFile, imageString);
                        addedCountry.FlagImage = imageString;
                        await _countryRepository.UpdateAsync(addedCountry);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Country was added successfully but image failed to be inserted");
                    }

                }
                Logs<Country>.AddEntityLog(_logger, "Country", country);
                var mappedCountry = _mapper.Map<CountryDto>(country);
                return mappedCountry;
            }
            catch (Exception ex)
            {
                Logs<Country>.AddEntityException(_logger, ex, "Country", _mapper.Map<Country>(countryToAdd));
                throw;
            }
        }

        private async Task<Country> GetCountry(Guid id)
        {
            var country = await _countryRepository.GetByIdAsync(id);
            if (country == null)
            {
                throw new ElementNotFoundException();
            }
            return country;
        }

        private async Task ValidateCountryAsync(Country country)
        {
            var validationResults = await _countryValidation.ValidateAsync(country);
            if (!validationResults.IsValid)
            {
                throw new ValidationException(validationResults);
            }
        }
    }
}
