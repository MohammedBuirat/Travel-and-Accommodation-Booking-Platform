using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using System.Linq.Expressions;
using Travel_and_Accommodation_API.DataAccess.Repositories.IRepository;
using Travel_and_Accommodation_API.Dto.Attraction;
using Travel_and_Accommodation_API.Exceptions_and_logs;
using Travel_and_Accommodation_API.Helpers;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Services.DataAccess.IServices;
using Travel_and_Accommodation_API.Services.ImageService;
using Travel_and_Accommodation_API.Services.Validation;

namespace Travel_and_Accommodation_API.Services.DataAccess.Services
{
    public class AttractionService : IAttractionService
    {
        private readonly IRepository<Attraction> _attractionRepository;
        private readonly ILogger<AttractionService> _logger;
        private readonly IImageService _imageService;
        private readonly IMapper _mapper;
        private readonly AttractionValidation _attractionValidation;
        private readonly IRepository<City> _cityRepository;


        public AttractionService(IRepository<Attraction> attractionRepository,
            ILogger<AttractionService> logger,
            IImageService imageService,
            IMapper mapper,
            AttractionValidation attractionValidation,
            IRepository<City> cityRepository)
        {
            _attractionRepository = attractionRepository ?? throw new ArgumentNullException(nameof(attractionRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _attractionValidation = attractionValidation ?? throw new ArgumentNullException(nameof(attractionValidation));
            _cityRepository = cityRepository ?? throw new ArgumentNullException(nameof(cityRepository));
        }

        public async Task<AttractionDto> GetByIdAsync(Guid id)
        {
            try
            {
                var attraction = await GetAttraction(id);
                var attractionDto = _mapper.Map<AttractionDto>(attraction);
                Logs<Attraction>.GetEntityLog(_logger, "Attraction", id);
                return attractionDto;
            }
            catch (ElementNotFoundException)
            {
                return null;
            }
            catch (Exception ex)
            {
                Logs<Attraction>.GetEntityException(_logger, ex, "Attraction", id);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            try
            {
                var attraction = await GetAttraction(id);
                await _attractionRepository.DeleteAsync(id);
                try
                {
                    await _imageService.DeleteFileAsync(attraction.Image);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Attraction was deleted but image deletion failed");
                }
                Logs<Attraction>.DeleteEntityLog(_logger, "Attraction", attraction);
            }
            catch (Exception ex)
            {
                Logs<Attraction>.DeleteEntityException(_logger, ex, "Attraction", id);
                throw;
            }
        }
        public async Task<IEnumerable<AttractionDto>> GetAllAsync(int? pageSize, int? pageNumber)
        {
            try
            {
                var paging = new Paging(pageSize, pageNumber);
                var attractions = await _attractionRepository.GetAllAsync(paging);
                var attractionDtos = _mapper.Map<List<AttractionDto>>(attractions);
                Logs<Attraction>.GetEntitiesLog(_logger, "Attractions");
                return attractionDtos;
            }
            catch (Exception ex)
            {
                Logs<Attraction>.GetEntitiesException(_logger, ex, "Attractions");
                throw;
            }
        }

        public async Task UpdateAsync(Guid id, AttractionToAdd entity)
        {
            try
            {
                var existingAttraction = await GetAttraction(id);
                var updatedAttraction = existingAttraction.Clone();
                _mapper.Map(entity, updatedAttraction);

                await ValidateAttraction(updatedAttraction);

                await _attractionRepository.UpdateAsync(updatedAttraction);
                Logs<Attraction>.UpdateEntityLog(_logger, "Attraction", existingAttraction);
                if (entity.ImageFile != null)
                {
                    string fileName = existingAttraction.Image;
                    await _imageService.UpdateFile(fileName, entity.ImageFile);
                }
            }
            catch (Exception ex)
            {
                Logs<Attraction>.DeleteEntityException(_logger, ex, "Attraction", id);
                throw;
            }
        }

        public async Task PartialUpdateAsync(Guid id, JsonPatchDocument<Attraction> patchDocument)
        {
            try
            {
                var existingAttraction = await GetAttraction(id);
                var updatedAttraction = existingAttraction.Clone();
                patchDocument.ApplyTo(existingAttraction);

                await ValidateAttraction(updatedAttraction);

                await _attractionRepository.UpdateAsync(updatedAttraction);
                Logs<Attraction>.UpdateEntityLog(_logger, "Attraction", existingAttraction);
            }
            catch (Exception ex)
            {
                Logs<Attraction>.UpdateEntityException(_logger, ex, "Attraction", id);
                throw;
            }
        }
        public async Task<AttractionDto> AddAsync(AttractionToAdd attractionToAdd)
        {
            try
            {
                var attraction = _mapper.Map<Attraction>(attractionToAdd);

                await ValidateAttraction(attraction);
                attraction.Id = Guid.NewGuid();
                await _attractionRepository.AddAsync(attraction);
                if (attractionToAdd.ImageFile != null)
                {
                    try
                    {
                        string imageString = $"attraction{attraction.Id}";
                        await _imageService.AddFileAsync(attractionToAdd.ImageFile, imageString);
                        attraction.Image = imageString;
                        await _attractionRepository.UpdateAsync(attraction);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Attraction was added successfully but image failed to be inserted");
                    }

                }
                Logs<Attraction>.AddEntityLog(_logger, "Attraction", attraction);
                var mappedAttraction = _mapper.Map<AttractionDto>(attraction);
                return mappedAttraction;

            }
            catch (Exception ex)
            {
                Logs<Attraction>.AddEntityException(_logger, ex, "Attraction", _mapper.Map<Attraction>(attractionToAdd));
                throw;
            }
        }

        public async Task<IEnumerable<AttractionDto>> GetCityAttractionsAsync(Guid cityId, int? pageSize, int? pageNumber)
        {
            try
            {
                var city = await _cityRepository.GetByIdAsync(cityId);
                if (city == null)
                {
                    throw new ElementNotFoundException();
                }
                Expression<Func<Attraction, bool>> ex = at => at.CityId == cityId;
                var customExpression = new CustomExpression<Attraction>();
                var paging = new Paging(pageSize, pageNumber);
                customExpression.Filter = ex;
                customExpression.Paging = paging;

                var attractions = await _attractionRepository.GetFilteredItemsAsync(customExpression);
                var attractionDtos = _mapper.Map<IEnumerable<AttractionDto>>(attractions);
                _logger.LogInformation($"Attraction for city {cityId} were retrieved");
                return attractionDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing Get attraction by city id request.");
                throw;
            }
        }

        private async Task<Attraction> GetAttraction(Guid id)
        {
            var attr = await _attractionRepository.GetByIdAsync(id);
            if (attr == null)
            {
                throw new ElementNotFoundException();
            }
            return attr;
        }

        private async Task ValidateAttraction(Attraction attraction)
        {
            var validationResults = await _attractionValidation.ValidateAsync(attraction);
            if (!validationResults.IsValid)
            {
                throw new ValidationException(validationResults);
            }
        }
    }
}
