using AutoMapper;
using System.Linq.Expressions;
using System.Security.Claims;
using Travel_and_Accommodation_API.DataAccess.Repositories.IRepository;
using Travel_and_Accommodation_API.Dto.CartBookingRoom;
using Travel_and_Accommodation_API.Exceptions_and_logs;
using Travel_and_Accommodation_API.Helpers;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Services.DataAccess.IServices;
using Travel_and_Accommodation_API.Services.Validation;

namespace Travel_and_Accommodation_API.Services.DataAccess.Services
{
    public class CartBookingRoomService : ICartBookingRoomService
    {
        private readonly IRepository<CartBookingRoom> _cartBookingRoomRepository;
        private readonly ILogger<CartBookingRoomService> _logger;
        private readonly IMapper _mapper;
        private readonly IRepository<CartBooking> _cartBookingRepository;
        private readonly CartBookingRoomValidation _cartBookingRoomValidation;

        public CartBookingRoomService(IRepository<CartBookingRoom> cartBookingRoomRepository,
            ILogger<CartBookingRoomService> logger,
            IMapper mapper,
            IRepository<CartBooking> cartBookingRepository,
            CartBookingRoomValidation cartBookingRoomValidation)
        {
            _cartBookingRoomRepository = cartBookingRoomRepository ?? throw new ArgumentNullException(nameof(cartBookingRoomRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _cartBookingRoomValidation = cartBookingRoomValidation ?? throw new ArgumentNullException(nameof(cartBookingRoomValidation));
            _cartBookingRepository = cartBookingRepository ?? throw new ArgumentNullException(nameof(cartBookingRepository));
        }

        public async Task<CartBookingRoom> AddAsync(CartBookingRoomDto entity, ClaimsPrincipal userClaims)
        {
            try
            {
                var bookingRoom = _mapper.Map<CartBookingRoom>(entity);
                var booking = await _cartBookingRepository.GetByIdAsync(entity.CartBookingId);
                if (booking != null)
                {
                    AuthorizeUser(userClaims, booking.UserId);
                }
                await ValidateBookingRoom(bookingRoom);
                Logs<CartBookingRoom>.AddEntityLog(_logger, "CartBookingRoom", bookingRoom);
                return bookingRoom;
            }
            catch (Exception ex)
            {
                Logs<CartBookingRoom>.AddEntityException(_logger, ex, "CartBookingRoom", _mapper.Map<CartBookingRoom>(entity));
                throw;
            }
        }


        public async Task DeleteAsync(Guid bookingId, Guid roomId, ClaimsPrincipal userClaims)
        {
            try
            {
                var bookingRoom = await GetCartBookingRoom(bookingId, roomId);
                var booking = await _cartBookingRepository.GetByIdAsync(bookingId);

                AuthorizeUser(userClaims, booking.UserId);
                await _cartBookingRoomRepository.DeleteAsync(bookingRoom.Id);
                Logs<CartBookingRoom>.DeleteEntityLog(_logger, "CartBookingRoom", bookingRoom);
            }
            catch (Exception ex)
            {
                Logs<CartBookingRoom>.DeleteEntityException(_logger, ex, "CartBookingRoom", bookingId, roomId);
                throw;
            }
        }

        public async Task<IEnumerable<CartBookingRoomDto>> GetBookingRoomsByBookingIdAsync(Guid bookingId, int? pageSize, int? pageNumber,
            ClaimsPrincipal userClaims)
        {
            try
            {
                var booking = await _cartBookingRepository.GetByIdAsync(bookingId);
                if (booking == null)
                {
                    throw new ElementNotFoundException();
                }

                AuthorizeUser(userClaims, booking.UserId);

                var paging = new Paging(pageSize, pageNumber);
                Expression<Func<CartBookingRoom, bool>> filter = br => br.CartBookingId == bookingId;
                var customExpression = new CustomExpression<CartBookingRoom> { Filter = filter, Paging = paging };
                var filteredRooms = await _cartBookingRoomRepository.GetFilteredItemsAsync(customExpression);

                var bookingRoomDtos = _mapper.Map<List<CartBookingRoomDto>>(filteredRooms);
                _logger.LogInformation($"cart Booking Room with Booking Id {bookingId} were retrieved");
                return bookingRoomDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing the request in GetcartBookingRoomsByBookingId method.");
                throw;
            }
        }

        private async Task ValidateBookingRoom(CartBookingRoom entity)
        {
            var validationResults = await _cartBookingRoomValidation.ValidateAsync(entity);
            if (!validationResults.IsValid)
            {
                throw new ValidationException(validationResults);
            }
        }

        private async Task<CartBookingRoom> GetCartBookingRoom(Guid bookingId, Guid roomId)
        {
            Expression<Func<CartBookingRoom, bool>> filter = br => br.CartBookingId == bookingId && br.RoomId == roomId;
            var bookingRoom = await _cartBookingRoomRepository.GetFirstOrDefaultAsync(filter);
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
