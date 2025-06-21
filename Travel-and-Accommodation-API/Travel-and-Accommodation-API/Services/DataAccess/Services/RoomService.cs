using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using Travel_and_Accommodation_API.DataAccess.Repositories.IRepository;
using Travel_and_Accommodation_API.DataAccess.Repositories.RepositoryImplementation;
using Travel_and_Accommodation_API.DataAccess.UnitOfWork.IUnitOfWorks;
using Travel_and_Accommodation_API.Dto.Room;
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
    public class RoomService : IRoomService
    {
        private readonly IRepository<Room> _roomRepository;
        private readonly ILogger<RoomService> _logger;
        private readonly IRoomDayService _roomDayService;
        private readonly IImageService _imageService;
        private readonly ISearchFilteredRoomsUnitOfWork _searchFilteredRoomsUnitOfWork;
        private readonly RoomValidation _roomValidation;
        private readonly IMapper _mapper;
        private readonly IRepository<Hotel> _hotelRepository;

        public RoomService(IRepository<Room> roomRepository,
            ILogger<RoomService> logger,
            IRoomDayService roomDayService,
            IImageService imageService,
            ISearchFilteredRoomsUnitOfWork searchFilteredRoomsUnitOfWork,
            RoomValidation roomValidation,
            IMapper mapper,
            IRepository<Hotel> hotelRepository)
        {
            _roomRepository = roomRepository ??
                throw new ArgumentNullException(nameof(roomRepository));
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
            _roomDayService = roomDayService ??
                throw new ArgumentNullException(nameof(roomDayService));
            _imageService = imageService ??
                throw new ArgumentNullException(nameof(imageService));
            _searchFilteredRoomsUnitOfWork = searchFilteredRoomsUnitOfWork ??
                throw new ArgumentNullException(nameof(searchFilteredRoomsUnitOfWork));
            _roomValidation = roomValidation ??
                throw new ArgumentNullException(nameof(roomValidation));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
            _hotelRepository = hotelRepository ??
                throw new ArgumentNullException(nameof(hotelRepository));
        }



        public async Task<IEnumerable<RoomDto>> GetAllAsync(int? pageSize, int? pageNumber)
        {
            try
            {
                var paging = new Paging(pageSize, pageNumber);
                var rooms = await _roomRepository.GetAllAsync(paging);
                var roomsToReturn = _mapper.Map<List<RoomDto>>(rooms);
                Logs<Room>.GetEntitiesLog(_logger, "Room");

                return roomsToReturn;
            }
            catch (Exception ex)
            {
                Logs<Room>.GetEntitiesException(_logger, ex, "Room");
                throw;
            }
        }

        public async Task<RoomDto> GetByIdAsync(Guid id)
        {
            try
            {
                var room = await GetRoomAsync(id);
                RoomDto roomToReturn = _mapper.Map<RoomDto>(room);
                Logs<Room>.GetEntityLog(_logger, "Room", id);
                return roomToReturn;
            }
            catch (Exception ex)
            {
                Logs<Room>.GetEntityException(_logger, ex, "Room", id);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            try
            {
                var room = await GetRoomAsync(id);

                await _roomRepository.DeleteAsync(id);
                Logs<Room>.DeleteEntityLog(_logger, "Room", room);
                try
                {
                    await _imageService.DeleteFileAsync(room.Image);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, $"While room was deleted image with path {room.Image} was not deleted");
                    throw new ExceptionWithMessage("Image was not updated");
                }
            }
            catch (Exception ex)
            {
                Logs<Room>.DeleteEntityException(_logger, ex, "Room", id);
                throw;
            }
        }

        public async Task UpdateAsync(Guid id, RoomToAdd roomToUpdate)
        {
            try
            {
                var oldRoom = await GetRoomAsync(id);
                var newRoom = oldRoom.Clone();
                newRoom.ModificationDate = DateTimeOffset.Now;
                _mapper.Map(roomToUpdate, newRoom);
                await ValidateRoomAsync(newRoom);

                await _roomRepository.UpdateAsync(oldRoom);
                Logs<Room>.UpdateEntityLog(_logger, "Room", oldRoom);
                if (roomToUpdate.ImageFile != null)
                {
                    try
                    {
                        string fileName = oldRoom.Image;
                        await _imageService.UpdateFile(fileName, roomToUpdate.ImageFile);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Image with path {oldRoom.Image} was not updated");
                        throw new ExceptionWithMessage("Image was not updated");
                    }
                }
            }
            catch (Exception ex)
            {
                Logs<Room>.UpdateEntityException(_logger, ex, "Room", id);
                throw;
            }
        }

        public async Task<Room> AddAsync(RoomToAdd room)
        {
            try
            {
                var roomToBeAdded = _mapper.Map<Room>(room);
                roomToBeAdded.CreationDate = DateTimeOffset.Now;
                await ValidateRoomAsync(roomToBeAdded);
                var addedRoom = await _roomRepository.AddAsync(roomToBeAdded);
                var imageString = $"room{addedRoom.Id}";
                try
                {
                    await _imageService.AddFileAsync(room.ImageFile, imageString);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Room was added but images was not added");
                    throw new ExceptionWithMessage("image was not added");
                }
                Logs<Room>.AddEntityLog(_logger, "Room", addedRoom);

                await _roomDayService.InsertRoomDaysAsync(addedRoom);
                return addedRoom;
            }
            catch (Exception ex)
            {
                Logs<Room>.AddEntityException(_logger, ex, "Room");
                throw;
            }
        }

        public async Task PartialUpdateAsync(Guid id, JsonPatchDocument<Room> jsonPatchDocument)
        {
            try
            {
                var oldRoom = await GetRoomAsync(id);
                var newRoom = oldRoom.Clone();
                jsonPatchDocument.ApplyTo(newRoom);

                await ValidateRoomAsync(newRoom);
                await _roomRepository.UpdateAsync(newRoom);
                Logs<Room>.UpdateEntityLog(_logger, "Room", oldRoom);
            }
            catch (Exception ex)
            {
                Logs<Room>.UpdateEntityException(_logger, ex, "Room", id);
                throw;
            }
        }

        public async Task<IEnumerable<RoomDto>> GetHotelRoomsAsync(Guid hotelId, int? pageSize, int? pageNumber)
        {
            try
            {
                var hotel = await _hotelRepository.GetByIdAsync(hotelId);
                if (hotel == null)
                {
                    throw new ElementNotFoundException();
                }

                var pagingInfo = new Paging(pageSize, pageNumber);
                Expression<Func<Room, bool>> filter = room => room.HotelId == hotelId;
                var customExpression = new CustomExpression<Room> { Filter = filter, Paging = pagingInfo };
                var rooms = await _roomRepository.GetFilteredItemsAsync(customExpression);
                List<RoomDto> roomsToReturn = _mapper.Map<List<RoomDto>>(rooms);
                return roomsToReturn;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing the GetHotelRooms request.");
                throw;
            }
        }

        public async Task<IEnumerable<RoomWithPriceDto>> GetSearchedFilteredRoomsAsync(Guid hotelId, int numberOfAdults, int numberOfChildren,
            DateTime checkInDate, DateTime checkOutDate, int? pageSize, int? pageNumber, [FromBody] List<string>? amenities,
            SortCriteria? sort, decimal? maxPrice, decimal? minPrice, RoomType? roomType, bool? descendingOrder)
        {
            try
            {
                var roomDayFilters = new List<Expression<Func<RoomDay, bool>>>
                {
                    roomDay => roomDay.Date.Date >= checkInDate.Date && roomDay.Date.Date < checkOutDate.Date,
                    roomDay => roomDay.Available
                };

                if (minPrice != null && minPrice != 0)
                    roomDayFilters.Add(roomDay => roomDay.Price >= minPrice);

                if (maxPrice != null)
                    roomDayFilters.Add(roomDay => roomDay.Price <= maxPrice);

                var roomFilters = new List<Expression<Func<Room, bool>>>
                {
                    room => room.HotelId == hotelId
                };
                if (amenities != null)
                {
                    var castedAminities = amenities.ConvertStringsToAmenities();
                    roomFilters.Add(room => ((long)room.Amenities & (long)castedAminities) == (long)castedAminities);
                }
                if (roomType != null)
                {
                    roomFilters.Add(room => room.Type == roomType);
                }
                if (numberOfChildren == 0)
                {
                    roomFilters.Add(room => room.AdultsCapacity >= numberOfAdults);
                }
                else
                {
                    roomFilters.Add(room => room.AdultsCapacity >= numberOfAdults && (room.AdultsCapacity + room.ChildrenCapacity) >= (numberOfAdults + numberOfChildren));
                }

                var filterExpression = new RoomPriceFilterExpression(
                    ExpressionCombiner.CombineExpressions(roomDayFilters),
                    ExpressionCombiner.CombineExpressions(roomFilters),
                    new Paging(pageSize, pageNumber),
                    sort,
                    descendingOrder);
                var rooms = await _searchFilteredRoomsUnitOfWork.GetSearchedFilteredRoomsAsync(filterExpression);
                var roomsToReturn = _mapper.Map<List<RoomWithPriceDto>>(rooms);
                return roomsToReturn;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetSearchedFilteredRooms: {ex.Message}");
                throw;
            }
        }

        private async Task<Room> GetRoomAsync(Guid id)
        {
            var room = await _roomRepository.GetByIdAsync(id);
            if (room == null)
            {
                throw new ElementNotFoundException();
            }
            return room;
        }

        private async Task ValidateRoomAsync(Room entity)
        {
            var validationResults = await _roomValidation.ValidateAsync(entity);
            if (!validationResults.IsValid)
            {
                throw new ValidationException(validationResults);
            }
        }
    }
}
