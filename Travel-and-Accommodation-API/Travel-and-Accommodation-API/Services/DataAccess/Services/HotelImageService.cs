using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Linq.Expressions;
using Travel_and_Accommodation_API.DataAccess.Repositories.IRepository;
using Travel_and_Accommodation_API.Dto.HotelImage;
using Travel_and_Accommodation_API.Exceptions_and_logs;
using Travel_and_Accommodation_API.Helpers;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Services.DataAccess.IServices;
using Travel_and_Accommodation_API.Services.ImageService;
using Travel_and_Accommodation_API.Services.Validation;

namespace Travel_and_Accommodation_API.Services.DataAccess.Services
{
    public class HotelImageService : IHotelImageService
    {
        private readonly IRepository<HotelImage> _hotelImageRepository;
        private readonly ILogger<HotelImageService> _logger;
        private readonly IMapper _mapper;
        private readonly HotelImageValidation _hotelImageValidation;
        private readonly IRepository<Hotel> _hotelRepository;
        private readonly IImageService _imageService;

        public HotelImageService(IRepository<HotelImage> hotelImageRepository,
            IMapper mapper,
            HotelImageValidation hotelImageValidation,
            IRepository<Hotel> hotelRepository,
            ILogger<HotelImageService> logger,
            IImageService imageService)
        {
            _hotelImageRepository = hotelImageRepository
                ?? throw new ArgumentNullException(nameof(hotelImageRepository));
            _mapper = mapper
                ?? throw new ArgumentNullException(nameof(mapper));
            _hotelImageValidation = hotelImageValidation
                ?? throw new ArgumentNullException(nameof(hotelImageValidation));
            _hotelRepository = hotelRepository
                ?? throw new ArgumentNullException(nameof(hotelRepository));
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            _imageService = imageService
                ?? throw new ArgumentNullException(nameof(imageService));
        }
        public async Task<IEnumerable<HotelImageDto>> GetAllAsync(int? pageSize, int? pageNumber)
        {
            try
            {
                var paging = new Paging(pageSize, pageNumber);
                var hotelImages = await _hotelImageRepository.GetAllAsync(paging);
                var hotelImagesToReturn = _mapper.Map<List<HotelImageDto>>(hotelImages);
                Logs<HotelImage>.GetEntitiesLog(_logger, "HotelImages");
                return hotelImagesToReturn;
            }
            catch (Exception ex)
            {
                Logs<HotelImage>.GetEntitiesException(_logger, ex, "HotelImages");
                throw;
            }
        }

        public async Task AddAsync([FromBody] HotelImageToAdd hotelImage)
        {
            try
            {
                var hotelImageToBeAdded = _mapper.Map<HotelImage>(hotelImage);
                await ValidateHotelImageAsync(hotelImageToBeAdded);

                var pageNumber = 1;
                var pageSize = 100;
                var page = new Paging(pageSize, pageNumber);
                var images = await _hotelImageRepository.GetAllAsync(page);
                var imageString = $"hotel_{hotelImage.HotelId}_{images.Count() + 1}";

                await _hotelImageRepository.AddAsync(hotelImageToBeAdded);
                Logs<HotelImage>.AddEntityLog(_logger, "HotelImage", hotelImageToBeAdded);
            }
            catch (Exception ex)
            {
                Logs<HotelImage>.AddEntityException(_logger, ex, "HotelImage", _mapper.Map<HotelImage>(hotelImage));
                throw;
            }
        }

        public async Task DeleteAsync(Guid hotelId, string imagePath)
        {
            try
            {
                var image = await GetHotelImg(hotelId, imagePath);

                await _hotelImageRepository.DeleteAsync(image.Id);
                await _imageService.DeleteFileAsync(imagePath);
                Logs<HotelImage>.DeleteEntityLog(_logger, "HotelImage", image);
            }
            catch (Exception ex)
            {
                Logs<HotelImage>.DeleteEntityException(_logger, ex, "HotelImage", hotelId);
                throw;
            }
        }
        public async Task<IEnumerable<HotelImageDto>> GetByHotelIdAsync(Guid hotelId)
        {
            try
            {
                var hotel = await _hotelRepository.GetByIdAsync(hotelId);
                if (hotel == null)
                {
                    throw new ElementNotFoundException();
                }
                var expression = new CustomExpression<HotelImage>();
                Expression<Func<HotelImage, bool>> filter = hi => hi.HotelId == hotelId;
                expression.Filter = filter;

                var images = await _hotelImageRepository.GetFilteredItemsAsync(expression);
                var imagesToReturn = _mapper.Map<List<HotelImageDto>>(images);
                return imagesToReturn;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching hotel images: {ex.Message}");
                throw;
            }
        }

        public async Task<HotelImageDto> GetAsync(Guid hotelId, string imagePath)
        {
            try
            {
                var hotelImage = await GetHotelImg(hotelId, imagePath);

                var image = await _imageService.GetFileAsync(imagePath);

                var imageToReturn = _mapper.Map<HotelImageDto>(image);
                return imageToReturn;
            }
            catch (ElementNotFoundException)
            {
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching hotel image: {ex.Message}");
                throw;
            }
        }


        private async Task<HotelImage> GetHotelImg(Guid hotelId, string path)
        {
            Expression<Func<HotelImage, bool>> filter = hi => hi.HotelId == hotelId && hi.ImageString == path;
            var hotelImage = await _hotelImageRepository.GetFirstOrDefaultAsync(filter);
            if (hotelImage == null)
            {
                throw new ElementNotFoundException();
            }
            return hotelImage;
        }

        private async Task ValidateHotelImageAsync(HotelImage hotelImage)
        {
            var validationResults = await _hotelImageValidation.ValidateAsync(hotelImage);
            if (!validationResults.IsValid)
            {
                throw new ValidationException(validationResults);
            }
        }
    }
}
