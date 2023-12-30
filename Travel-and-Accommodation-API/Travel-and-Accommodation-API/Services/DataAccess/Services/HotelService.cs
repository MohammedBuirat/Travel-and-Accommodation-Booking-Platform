using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Linq.Expressions;
using System.Security.Claims;
using Travel_and_Accommodation_API.DataAccess.Repositories.IRepository;
using Travel_and_Accommodation_API.DataAccess.UnitOfWork.IUnitOfWorks;
using Travel_and_Accommodation_API.Dto.Hotel;
using Travel_and_Accommodation_API.Exceptions_and_logs;
using Travel_and_Accommodation_API.Helpers;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Services.DataAccess.IServices;
using Travel_and_Accommodation_API.Services.ImageService;
using Travel_and_Accommodation_API.Services.Validation;
using Travel_and_Accommodation_API.Views;
using static Travel_and_Accommodation_API.Models.Enums;

namespace Travel_and_Accommodation_API.Services.DataAccess.Services
{
    public class HotelService : IHotelService
    {
        private readonly IRepository<Hotel> _hotelRepository;
        private readonly ILogger<HotelService> _logger;
        private readonly IImageService _imageService;
        private readonly IReviewRepository _reviewRepository;
        private readonly IMemoryCache _memoryCache;
        private readonly IGetTopDiscountedHotelsUnitOfWork _discountedHotelsUnitOfWork;
        private readonly string DiscountedHotels = "DiscountedHotels";
        private readonly ISearchedFilteredHotelsUnitOfWork _searchedFilteredHotelsUnitOfWork;
        private readonly HotelValidation _hotelValidation;
        private readonly IRepository<HotelImage> _hotelImageRepository;
        private readonly IMapper _mapper;
        private readonly IRepository<City> _cityRepository;
        private readonly IUserLastHotelsService _userLastHotelsService;

        public HotelService(
            IRepository<Hotel> hotelRepository,
            ILogger<HotelService> logger, IImageService imageService,
            IReviewRepository reviewRepository,
            IMemoryCache memoryCache,
            IGetTopDiscountedHotelsUnitOfWork discountedHotelsUnitOfWork,
            ISearchedFilteredHotelsUnitOfWork searchedFilteredHotelsUnitOfWork,
            HotelValidation hotelValidation,
            IRepository<HotelImage> hotelImageRepository,
            IMapper mapper, IRepository<City> cityRepository, IUserLastHotelsService userLastHotelsService)
        {
            _logger = logger;
            _hotelRepository = hotelRepository;
            _imageService = imageService ??
                throw new ArgumentNullException(nameof(imageService));
            _reviewRepository = reviewRepository ??
                throw new ArgumentNullException(nameof(reviewRepository));
            _memoryCache = memoryCache ??
                throw new ArgumentNullException(nameof(memoryCache));
            _discountedHotelsUnitOfWork = discountedHotelsUnitOfWork ??
                throw new ArgumentNullException(nameof(discountedHotelsUnitOfWork));
            _searchedFilteredHotelsUnitOfWork = searchedFilteredHotelsUnitOfWork ??
                throw new ArgumentNullException(nameof(searchedFilteredHotelsUnitOfWork));
            _hotelValidation = hotelValidation ??
                throw new ArgumentNullException(nameof(hotelValidation));
            _hotelImageRepository = hotelImageRepository ??
                throw new ArgumentNullException(nameof(hotelImageRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
            _cityRepository = cityRepository ??
                throw new ArgumentNullException(nameof(cityRepository));
            _userLastHotelsService = userLastHotelsService ??
                throw new ArgumentNullException(nameof(userLastHotelsService));
        }
        public async Task<IEnumerable<HotelDto>> GetAllAsync(int? pageSize, int? pageNumber)
        {
            try
            {
                var paging = new Paging(pageSize, pageNumber);
                var hotels = await _hotelRepository.GetAllAsync(paging);
                var hotelsToBeReturned = _mapper.Map<List<HotelDto>>(hotels);
                Logs<Hotel>.GetEntitiesLog(_logger, "Hotel");
                return hotelsToBeReturned;
            }
            catch (Exception ex)
            {
                Logs<Hotel>.GetEntitiesException(_logger, ex, "Hotel");
                throw;
            }
        }

        public async Task<ActionResult<HotelDto>> GetByIdAsync(Guid id, ClaimsPrincipal userClaims)
        {
            try
            {
                var hotel = await GetHotel(id);
                var userId = userClaims.Claims.FirstOrDefault(c => c.Type == "Sub")?.Value;
                if (userId != null)
                {
                    await _userLastHotelsService.AddAsync(new UserLastHotels
                    {
                        HotelId = hotel.Id,
                        UserId = userId
                    });
                }
                var hotelToBeReturned = _mapper.Map<HotelDto>(hotel);
                Logs<Hotel>.GetEntityLog(_logger, "Hotel", id);
                return hotelToBeReturned;
            }
            catch (Exception ex)
            {
                Logs<Hotel>.GetEntityException(_logger, ex, "Hotel", id);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            try
            {
                var hotel = await GetHotel(id);
                var images = await _hotelImageRepository.GetFilteredItemsAsync(hi => hi.HotelId == id);
                var imagesPath = images.Select(image => image.ImageString);
                await _hotelRepository.DeleteAsync(id);
                Logs<Hotel>.DeleteEntityLog(_logger, "Hotel", hotel);
                try
                {
                    await _imageService.DeleteMultipleFilesAsync(imagesPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"While hotel was deleted images were not deleted");
                    throw new ExceptionWithMessage("Images were not updated");
                }
            }
            catch (Exception ex)
            {
                Logs<Hotel>.DeleteEntityException(_logger, ex, "Hotel", id);
                throw;
            }
        }

        public async Task UpdateAsync(Guid id, HotelToAdd hotelToUpdate)
        {
            try
            {
                var oldHotel = await GetHotel(id);
                var newHotel = oldHotel.Clone();
                newHotel.ModificationDate = DateTimeOffset.Now;
                newHotel = _mapper.Map<Hotel>(hotelToUpdate);

                await ValidateHotelAsync(newHotel);

                await _hotelRepository.UpdateAsync(oldHotel);
                Logs<Hotel>.UpdateEntityLog(_logger, "Hotel", oldHotel);
                if (hotelToUpdate.ImageFiles != null && hotelToUpdate.ImageFiles.Any())
                {
                    try
                    {
                        var imageStrings = hotelToUpdate.ImageFiles.Select((_, i) => $"hotel{id}_{i}");
                        await _imageService.UpdateMultipleFilesAsync(hotelToUpdate.ImageFiles, imageStrings);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Images for hotel with id {id} were not updated");
                        throw new ExceptionWithMessage("Images were not updated");
                    }
                }
            }
            catch (Exception ex)
            {
                Logs<Hotel>.UpdateEntityException(_logger, ex, "Hotel", id);
                throw;
            }
        }

        public async Task<Hotel> AddAsync(HotelToAdd hotel)
        {
            try
            {
                var hotelToBeAdded = _mapper.Map<Hotel>(hotel);

                await ValidateHotelAsync(hotelToBeAdded);
                hotelToBeAdded.CreationDate = DateTimeOffset.Now;
                var addedHotel = await _hotelRepository.AddAsync(hotelToBeAdded);
                var hotelImages = new List<HotelImage>();
                var files = hotel.ImageFiles;
                for (int i = 0; i < files.Count(); i++)
                {
                    var image = new HotelImage
                    {
                        HotelId = addedHotel.Id,
                        ImageString = $"hotel{addedHotel.Id}_{i}"
                    };
                    hotelImages.Add(image);
                }
                await _imageService.AddMultipleFilesAsync(files, hotelImages.Select(hi => hi.ImageString));
                await _hotelImageRepository.AddAllAsync(hotelImages);
                Logs<Hotel>.AddEntityLog(_logger, "Hotel", addedHotel);
                return addedHotel;
            }
            catch (Exception ex)
            {
                Logs<Hotel>.AddEntityException(_logger, ex, "Hotel");
                throw;
            }
        }

        public async Task PartialUpdateAsync(Guid id, JsonPatchDocument<Hotel> jsonPatchDocument)
        {
            try
            {
                var oldHotel = await GetHotel(id);
                var updatedHotel = oldHotel.Clone();
                jsonPatchDocument.ApplyTo(updatedHotel);
                updatedHotel.ModificationDate = DateTimeOffset.Now;
                await ValidateHotelAsync(updatedHotel);
                await _hotelRepository.UpdateAsync(updatedHotel);
                Logs<Hotel>.UpdateEntityLog(_logger, "Hotel", oldHotel);
            }
            catch (Exception ex)
            {
                Logs<Hotel>.UpdateEntityException(_logger, ex, "Hotel", id);
                throw;
            }
        }

        public async Task<IEnumerable<HotelDto>> GetCityHotelsAsync(Guid cityId, int? pageSize, int? pageNum)
        {
            try
            {
                var city = await _cityRepository.GetByIdAsync(cityId);
                if (city == null)
                {
                    throw new ElementNotFoundException();
                }

                var expression = new CustomExpression<Hotel>();
                var paging = new Paging(pageSize, pageNum);
                expression.Paging = paging;
                Expression<Func<Hotel, bool>> filter = hotel => hotel.CityId == cityId;
                expression.Filter = filter;

                var hotels = await _hotelRepository.GetFilteredItemsAsync(expression);
                var hotelsToBeReturned = _mapper.Map<List<HotelDto>>(hotels);

                return hotelsToBeReturned;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing the GetCityHotels request.");
                throw;
            }
        }

        public async Task<ActionResult<IEnumerable<HotelDto>>> AdminSearchHotelsAsync(string? name, int? pageSize, int? pageNumber,
            Guid? cityId, DateTimeOffset? creationDate, decimal? ratings)
        {
            try
            {
                string searchTermUpper = name?.ToUpper() ?? string.Empty;
                List<Expression<Func<Hotel, bool>>> expressions = new List<Expression<Func<Hotel, bool>>>();

                if (!string.IsNullOrEmpty(name))
                {
                    Expression<Func<Hotel, bool>> exp = hotel => hotel.Name.ToUpper() == searchTermUpper;
                    expressions.Add(exp);
                }

                if (creationDate != null)
                {
                    Expression<Func<Hotel, bool>> exp = hotel => hotel.CreationDate >= creationDate;
                    expressions.Add(exp);
                }

                if (cityId != null)
                {
                    Expression<Func<Hotel, bool>> exp = hotel => hotel.CityId == cityId;
                    expressions.Add(exp);
                }

                if (ratings != null)
                {
                    Expression<Func<Hotel, bool>> exp = hotel => (hotel.SumOfRatings / hotel.NumOfRatings) >= ratings;
                    expressions.Add(exp);
                }

                var paging = new Paging(pageSize, pageNumber);
                var expression = new CustomExpression<Hotel>();
                expression.Filter = ExpressionCombiner.CombineExpressions(expressions);
                expression.Paging = paging;

                var hotels = await _hotelRepository.GetFilteredItemsAsync(expression);
                var hotelsToBeReturned = _mapper.Map<List<HotelDto>>(hotels);


                return hotelsToBeReturned;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching for hotels.");
                throw;
            }
        }

        public async Task<IEnumerable<HotelRoomWithDiscount>> GetDiscountedHotelRoomsAsync(int? pageSize, int? pageNumber)
        {
            try
            {
                var paging = new Paging(pageSize, pageNumber);
                IEnumerable<HotelRoomWithDiscount> hotels;
                var cachedRequest = $"{DiscountedHotels}_{paging.PageSize}_{paging.PageNumber}";
                if (!_memoryCache.TryGetValue(cachedRequest, out hotels))
                {
                    hotels = await _discountedHotelsUnitOfWork.GetTopDiscountedHotelsAsync(paging);
                    _memoryCache.Set(cachedRequest, hotels,
                        new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(1)));
                }
                return hotels;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetDiscountedHotelRooms: {ex.Message}");
                throw;
            }
        }

        //public async Task<IEnumerable<HotelWithPriceDto>> GetSearchedFilteredHotelsAsync(Guid cityId, int numberOfAdults, int numberOfChildren,
        //    DateTime checkInDate, DateTime checkOutDate, int? pageSize, int? pageNumber, List<string>? amenities,
        //    SortCriteria? sort, decimal? maxPrice, decimal? minPrice, int? minRatings, RoomType? roomType,
        //    bool? descendingOrder, decimal? distanceFromCityCenter)
        //{
        //    try
        //    {
        //        var roomDayFilters = new List<Expression<Func<RoomDay, bool>>>
        //        {
        //            roomDay => roomDay.Date.Date >= checkInDate.Date && roomDay.Date.Date < checkOutDate.Date && roomDay.Available
        //        };

        //        if (minPrice != null && minPrice != 0)
        //            roomDayFilters.Add(roomDay => roomDay.Price >= minPrice);

        //        if (maxPrice != null)
        //            roomDayFilters.Add(roomDay => roomDay.Price <= maxPrice);

        //        var roomFilters = new List<Expression<Func<Room, bool>>>();
        //        if (amenities != null && amenities.Count != 0)
        //        {
        //            var castedAminities = amenities.ConvertStringsToAmenities();
        //            roomFilters.Add(room => ((long)room.Amenities & (long)castedAminities) == (long)castedAminities);
        //        }
        //        if (roomType != null)
        //        {
        //            roomFilters.Add(room => room.Type == roomType);
        //        }
        //        if (numberOfChildren == 0)
        //        {
        //            roomFilters.Add(room => room.AdultsCapacity >= numberOfAdults);
        //        }
        //        else
        //        {
        //            roomFilters.Add(room => room.AdultsCapacity >= numberOfAdults && (room.AdultsCapacity + room.ChildrenCapacity) >= (numberOfAdults + numberOfChildren));
        //        }

        //        var hotelFilters = new List<Expression<Func<Hotel, bool>>>();
        //        hotelFilters.Add(hotel => hotel.CityId == cityId);
        //        if (distanceFromCityCenter != null)
        //        {
        //            hotelFilters.Add(hotel => hotel.DistanceFromCityCenter >= distanceFromCityCenter);
        //        }
        //        if (minRatings != null && minRatings != 0)
        //        {
        //            hotelFilters.Add(hotel => hotel.NumOfRatings != 0 && (hotel.SumOfRatings / hotel.NumOfRatings) >= minRatings);

        //        }

        //        var filterExpression = new HotelPriceFilterExpression(ExpressionCombiner.CombineExpressions(roomDayFilters),
        //            ExpressionCombiner.CombineExpressions(roomFilters),
        //            ExpressionCombiner.CombineExpressions(hotelFilters),
        //            descendingOrder ?? false,
        //            sort,
        //            new Paging(pageSize, pageNumber));

        //        var hotels = await _searchedFilteredHotelsUnitOfWork.GetSearchedFilteredHotelsAsync(filterExpression);
        //        var hotelsToReturn = _mapper.Map<List<HotelWithPriceDto>>(hotels);
        //        return hotelsToReturn;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Error in GetSearchedFilteredHotels: {ex.Message}");
        //        throw;
        //    }
        //}

        public async Task<IEnumerable<HotelWithPriceDto>> GetSearchedFilteredHotelsAsync(Guid cityId, int numberOfAdults, int numberOfChildren,
            DateTime checkInDate, DateTime checkOutDate, int? pageSize, int? pageNumber, List<string>? amenities,
            SortCriteria? sort, decimal? maxPrice, decimal? minPrice, int? minRatings, RoomType? roomType,
            bool? descendingOrder, decimal? distanceFromCityCenter)
        {
            try
            {
                var request = new FilteredHotelRequest(cityId, numberOfAdults, numberOfChildren,
            checkInDate, checkOutDate, pageSize, pageNumber, amenities,
            sort, maxPrice, minPrice, minRatings, roomType,
            descendingOrder, distanceFromCityCenter);
                var hotels = await _searchedFilteredHotelsUnitOfWork.GetSearchedFilteredHotelsAsync(request);
                var hotelsToReturn = _mapper.Map<List<HotelWithPriceDto>>(hotels);
                return hotelsToReturn;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

            public async Task EditHotelTotalReviewsAsync(Guid hotelId)
        {
            var hotel = await _hotelRepository.GetByIdAsync(hotelId);
            var hotelNumAndSumOfReviews = await _reviewRepository.GetHotelNumAndSumOfReviewsAsync(hotelId);
            hotel.NumOfRatings = hotelNumAndSumOfReviews.NumOfReviews;
            hotel.SumOfRatings = hotelNumAndSumOfReviews.SumOfReviews;
            await _hotelRepository.UpdateAsync(hotel);
        }

        private async Task<Hotel> GetHotel(Guid id)
        {
            var hotel = await _hotelRepository.GetByIdAsync(id);
            if (hotel == null)
            {
                throw new ElementNotFoundException();
            }
            return hotel;
        }

        private async Task ValidateHotelAsync(Hotel hotel)
        {
            var validationResults = await _hotelValidation.ValidateAsync(hotel);
            if (!validationResults.IsValid)
            {
                throw new ValidationException(validationResults);
            }
        }
    }
}
