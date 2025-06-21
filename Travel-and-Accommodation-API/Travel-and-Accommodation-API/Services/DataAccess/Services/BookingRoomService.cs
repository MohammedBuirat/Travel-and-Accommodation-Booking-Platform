using AutoMapper;
using System.Linq.Expressions;
using System.Security.Claims;
using Travel_and_Accommodation_API.DataAccess.Repositories.IRepository;
using Travel_and_Accommodation_API.DataAccess.UnitOfWork.IUnitOfWorks;
using Travel_and_Accommodation_API.Dto.BookingRoom;
using Travel_and_Accommodation_API.Exceptions_and_logs;
using Travel_and_Accommodation_API.Helpers;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Services.DataAccess.IServices;
using Travel_and_Accommodation_API.Services.Validation;

namespace Travel_and_Accommodation_API.Services.DataAccess.Services
{
    public class BookingRoomService : IBookingRoomService
    {
        private readonly IRepository<BookingRoom> _bookingRoomrepository;
        private readonly ILogger<BookingRoomService> _logger;
        private readonly IBookingRoomUnitOfWork _bookingRoomUnitOfWork;
        private readonly IMapper _mapper;
        private readonly IRepository<Booking> _bookingRepository;
        private readonly BookingRoomValidation _bookingRoomValidation;
        private readonly IRepository<Room> _roomRepostiory;

        public BookingRoomService(IRepository<BookingRoom> bookingRoomRepository,
            ILogger<BookingRoomService> logger,
            IBookingRoomUnitOfWork bookingRoomUnitOfWork,
            IMapper mapper,
            IRepository<Booking> bookingRepository,
            BookingRoomValidation bookingRoomValidation,
            IRepository<Room> roomRepostiory)
        {
            _bookingRoomrepository = bookingRoomRepository ?? throw new ArgumentNullException(nameof(bookingRoomRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _bookingRoomUnitOfWork = bookingRoomUnitOfWork ?? throw new ArgumentNullException(nameof(bookingRoomUnitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _bookingRoomValidation = bookingRoomValidation ?? throw new ArgumentNullException(nameof(bookingRoomValidation));
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            _roomRepostiory = roomRepostiory ?? throw new ArgumentNullException(nameof(roomRepostiory));
        }

        public async Task<BookingRoom> AddAsync(BookingRoomDto entity, ClaimsPrincipal userClaims)
        {
            try
            {
                var bookingRoom = _mapper.Map<BookingRoom>(entity);
                var booking = await _bookingRepository.GetByIdAsync(entity.BookingId);
                if (booking != null)
                {
                    AuthorizeUser(userClaims, booking.UserId);
                }
                await ValidateBookingRoom(bookingRoom);
                Logs<BookingRoom>.AddEntityLog(_logger, "BookingRoom", bookingRoom);
                return bookingRoom;
            }
            catch (Exception ex)
            {
                Logs<BookingRoom>.AddEntityException(_logger, ex, "Booking", _mapper.Map<BookingRoom>(entity));
                throw;
            }
        }


        public async Task DeleteAsync(Guid bookingId, Guid roomId, ClaimsPrincipal userClaims)
        {
            try
            {
                var bookingRoom = await GetBookingRoom(bookingId, roomId);
                var booking = await _bookingRepository.GetByIdAsync(bookingId);

                AuthorizeUser(userClaims, booking.UserId);
                await _bookingRoomUnitOfWork.DeleteBookingRoomAsync(bookingRoom.Id);
                Logs<BookingRoom>.DeleteEntityLog(_logger, "BookingRoom", bookingRoom);
            }
            catch (Exception ex)
            {
                Logs<BookingRoom>.DeleteEntityException(_logger, ex, "BookingRoom", bookingId, roomId);
                throw;
            }
        }

        public async Task<IEnumerable<BookingRoomDto>> GetBookingRoomsByBookingIdAsync(Guid bookingId, int? pageSize, int? pageNumber,
            ClaimsPrincipal userClaims)
        {
            try
            {
                var booking = await _bookingRepository.GetByIdAsync(bookingId);
                if (booking == null)
                {
                    throw new ElementNotFoundException();
                }

                AuthorizeUser(userClaims, booking.UserId);

                var paging = new Paging(pageSize, pageNumber);
                Expression<Func<BookingRoom, bool>> filter = br => br.BookingId == bookingId;
                var customExpression = new CustomExpression<BookingRoom> { Filter = filter, Paging = paging };
                var filteredRooms = await _bookingRoomrepository.GetFilteredItemsAsync(customExpression);

                var bookingRoomDtos = _mapper.Map<List<BookingRoomDto>>(filteredRooms);
                _logger.LogInformation($"Booking Room with Booking Id {bookingId} were retrieved");
                return bookingRoomDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing the request in GetBookingRoomsByBookingId method.");
                throw;
            }
        }

        public async Task<IEnumerable<BookingRoomDto>> GetBookingRoomsByRoomIdIdAsync(Guid roomId, int? pageSize, int? pageNumber,
            ClaimsPrincipal userClaims)
        {
            try
            {
                var booking = await _roomRepostiory.GetByIdAsync(roomId);
                if (booking == null)
                {
                    throw new ElementNotFoundException();
                }

                var paging = new Paging(pageSize, pageNumber);
                Expression<Func<BookingRoom, bool>> filter = br => br.RoomId == roomId;
                var customExpression = new CustomExpression<BookingRoom> { Filter = filter, Paging = paging };
                var filteredRooms = await _bookingRoomrepository.GetFilteredItemsAsync(customExpression);

                var bookingRoomDtos = _mapper.Map<List<BookingRoomDto>>(filteredRooms);
                _logger.LogInformation($"Booking Room with Booking Id {roomId} were retrieved");
                return bookingRoomDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing the request in GetBookingRoomsByRoomId method.");
                throw;
            }
        }

        private async Task ValidateBookingRoom(BookingRoom entity)
        {
            var validationResults = await _bookingRoomValidation.ValidateAsync(entity);
            if (!validationResults.IsValid)
            {
                throw new ValidationException(validationResults);
            }
        }

        private async Task<BookingRoom> GetBookingRoom(Guid bookingId, Guid roomId)
        {
            Expression<Func<BookingRoom, bool>> filter = br => br.BookingId == bookingId && br.RoomId == roomId;
            var bookingRoom = await _bookingRoomrepository.GetFirstOrDefaultAsync(filter);
            if (bookingRoom == null)
            {
                throw new ElementNotFoundException();
            }
            return bookingRoom;
        }
        private void AuthorizeUser(ClaimsPrincipal userClaims, string id)
        {
            var userId = userClaims.Claims.FirstOrDefault(c => c.Type == "Sub")?.Value;
            var userRole = userClaims.Claims.FirstOrDefault(c => c.Type == "Role")?.Value ?? "User";
            if (userId != id && userRole != "Admin")
            {
                throw new UnauthorizedAccessException();
            }
        }

    }
}
