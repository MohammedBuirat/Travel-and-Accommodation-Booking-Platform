using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Caching.Memory;
using System.Linq.Expressions;
using Travel_and_Accommodation_API.DataAccess.Repositories.IRepository;
using Travel_and_Accommodation_API.DataAccess.UnitOfWork.IUnitOfWorks;
using Travel_and_Accommodation_API.Dto.City;
using Travel_and_Accommodation_API.Exceptions_and_logs;
using Travel_and_Accommodation_API.Helpers;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Services.DataAccess.IServices;
using Travel_and_Accommodation_API.Services.ImageService;
using Travel_and_Accommodation_API.Services.Validation;

namespace Travel_and_Accommodation_API.Services.DataAccess.Services
{
    public class CityService : ICityService
    {
        private readonly IRepository<City> _cityRepository;
        private readonly ILogger<CityService> _logger;
        private readonly IImageService _imageService;
        private readonly string MostCities = "MostVisitedCities";
        private readonly IMemoryCache _memoryCache;
        private readonly IMostVisitedCitiesUnitOfWork _mostVisitedCitiesUnitOfWork;
        private readonly CityValidation _cityValidation;
        private readonly IMapper _mapper;


        public CityService(IRepository<City> cityRepository,
            ILogger<CityService> logger,
            IImageService imageService,
            IMemoryCache memoryCache,
            IMostVisitedCitiesUnitOfWork mostVisitedCitiesUnitOfWork,
            CityValidation cityValidation,
            IMapper mapper)
        {
            _cityRepository = cityRepository
                ?? throw new ArgumentNullException(nameof(cityRepository));
            _logger = logger;
            _imageService = imageService
                ?? throw new ArgumentNullException(nameof(imageService));
            _memoryCache = memoryCache
                ?? throw new ArgumentNullException(nameof(memoryCache));
            _mostVisitedCitiesUnitOfWork = mostVisitedCitiesUnitOfWork
                ?? throw new ArgumentNullException(nameof(mostVisitedCitiesUnitOfWork));
            _cityValidation = cityValidation
                ?? throw new ArgumentNullException(nameof(cityValidation));
            _mapper = mapper
                ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<CityDto> GetByIdAsync(Guid id)
        {
            try
            {
                var city = await GetCity(id);
                var cityDto = _mapper.Map<CityDto>(city);
                Logs<City>.GetEntityLog(_logger, "City", id);
                return cityDto;
            }
            catch (ElementNotFoundException)
            {
                return null;
            }
            catch (Exception ex)
            {
                Logs<City>.GetEntityException(_logger, ex, "City", id);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            try
            {
                var city = await GetCity(id);
                await _cityRepository.DeleteAsync(id);
                if (!string.IsNullOrEmpty(city.Image))
                {
                    await _imageService.DeleteFileAsync(city.Image);
                }
                Logs<City>.DeleteEntityLog(_logger, "City", city);
            }
            catch (Exception ex)
            {
                Logs<City>.DeleteEntityException(_logger, ex, "City", id);
                throw;
            }
        }

        public async Task<IEnumerable<CityDto>> GetAllAsync(int? pageSize, int? pageNumber)
        {
            try
            {
                var paging = new Paging(pageSize, pageNumber);
                var cities = await _cityRepository.GetAllAsync(paging);
                var cityDtos = _mapper.Map<List<CityDto>>(cities);
                Logs<City>.GetEntitiesLog(_logger, "Cities");
                return cityDtos;
            }
            catch (Exception ex)
            {
                Logs<City>.GetEntitiesException(_logger, ex, "Cities");
                throw;
            }
        }

        public async Task UpdateAsync(Guid id, CityToAdd entity)
        {
            try
            {
                var existingCity = await GetCity(id);
                var updatedCity = existingCity.Clone();
                _mapper.Map(entity, updatedCity);

                await ValidateCityAsync(updatedCity);

                updatedCity.ModificationDate = DateTime.Now;
                await _cityRepository.UpdateAsync(updatedCity);
                if (entity.ImageFile != null)
                {
                    string fileName = existingCity.Image;
                    await _imageService.UpdateFile(fileName, entity.ImageFile);
                }
                Logs<City>.UpdateEntityLog(_logger, "City", existingCity);
            }
            catch (Exception ex)
            {
                Logs<City>.DeleteEntityException(_logger, ex, "City", id);
                throw;
            }
        }

        public async Task PartialUpdateAsync(Guid id, JsonPatchDocument<City> patchDocument)
        {
            try
            {
                var existingCity = await GetCity(id);
                var updatedCity = existingCity.Clone();
                patchDocument.ApplyTo(existingCity);

                await ValidateCityAsync(updatedCity);

                updatedCity.ModificationDate = DateTime.Now;
                await _cityRepository.UpdateAsync(updatedCity);
                Logs<City>.UpdateEntityLog(_logger, "City", existingCity);
            }
            catch (Exception ex)
            {
                Logs<City>.UpdateEntityException(_logger, ex, "City", id);
                throw;
            }
        }

        public async Task<CityDto> AddAsync(CityToAdd cityToAdd)
        {
            try
            {
                var city = _mapper.Map<City>(cityToAdd);
                city.CreationDate = DateTimeOffset.Now;
                await ValidateCityAsync(city);
                var addedCity = await _cityRepository.AddAsync(city);
                if (cityToAdd.ImageFile != null)
                {
                    try
                    {
                        string imageString = $"city{addedCity.Id}";
                        await _imageService.AddFileAsync(cityToAdd.ImageFile, imageString);
                        addedCity.Image = imageString;
                        await _cityRepository.UpdateAsync(addedCity);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "City was added successfully but the image failed to be inserted");
                    }
                }
                Logs<City>.AddEntityLog(_logger, "City", city);
                var mappedCity = _mapper.Map<CityDto>(city);
                return mappedCity;
            }
            catch (Exception ex)
            {
                Logs<City>.AddEntityException(_logger, ex, "City", _mapper.Map<City>(cityToAdd));
                throw;
            }
        }

        public async Task<IEnumerable<City>> MostVisitedCitiesAsync(int? pageSize, int? pageNumber)
        {
            try
            {
                IEnumerable<City> cities;
                var paging = new Paging(pageSize, pageNumber);
                if (!_memoryCache.TryGetValue(MostCities + "_" + paging.PageNumber + "_" + paging.PageSize, out cities))
                {
                    cities = await _mostVisitedCitiesUnitOfWork.MostVisitedCitiesAsync(paging);
                    _memoryCache.Set(MostCities, cities,
                        new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromDays(1)));
                }
                return cities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the most visited cities");
                return Enumerable.Empty<City>();
            }
        }

        public async Task<IEnumerable<CityDto>> SearchCitiesAsync(string name, int? pageSize, int? pageNumber)
        {
            try
            {
                string searchTermUpper = name.ToUpper();
                Expression<Func<City, bool>> searchExpression = city => city.Name.ToUpper().Contains(searchTermUpper);
                var page = new Paging(pageSize, pageNumber);
                var expression = new CustomExpression<City> { Filter = searchExpression, Paging = page };

                var cities = await _cityRepository.GetFilteredItemsAsync(expression);
                var citiesToReturn = _mapper.Map<List<CityDto>>(cities);


                return citiesToReturn;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching for cities.");
                throw;
            }
        }

        public async Task<IEnumerable<CityDto>> AdminCitiesSearchAsync(
            string? name, int? pageSize, int? pageNumber, Guid? country, DateTimeOffset? creationDate)
        {
            try
            {
                List<Expression<Func<City, bool>>> expressions = new List<Expression<Func<City, bool>>>();

                if (name != null)
                {
                    expressions.Add(city => city.Name.ToUpper().Contains(name.ToUpper()));
                }

                if (country != null)
                {
                    expressions.Add(city => city.CountryId == country);
                }

                if (creationDate != null)
                {
                    expressions.Add(city => city.CreationDate >= creationDate);
                }

                Expression<Func<City, bool>> combinedExpression = ExpressionCombiner.CombineExpressions(expressions);

                var page = new Paging(pageSize, pageNumber);
                var expression = new CustomExpression<City> { Filter = combinedExpression, Paging = page };

                var cities = await _cityRepository.GetFilteredItemsAsync(expression);
                var citiesToReturn = _mapper.Map<List<CityDto>>(cities);

                _logger.LogInformation($"Found {citiesToReturn.Count} matching cities.");

                return citiesToReturn;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching for cities.");
                throw;
            }
        }
        private async Task<City> GetCity(Guid id)
        {
            var city = await _cityRepository.GetByIdAsync(id);
            if (city == null)
            {
                throw new ElementNotFoundException();
            }
            return city;
        }

        private async Task ValidateCityAsync(City city)
        {
            var validationResults = await _cityValidation.ValidateAsync(city);
            if (!validationResults.IsValid)
            {
                throw new ValidationException(validationResults);
            }
        }
    }
}
