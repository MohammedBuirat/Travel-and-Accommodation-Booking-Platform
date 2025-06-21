using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using System.Security.Claims;
using Travel_and_Accommodation_API.DataAccess.Repositories.IRepository;
using Travel_and_Accommodation_API.Dto.CartBooking;
using Travel_and_Accommodation_API.Exceptions_and_logs;
using Travel_and_Accommodation_API.Helpers;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Services.DataAccess.IServices;
using Travel_and_Accommodation_API.Services.Validation;

namespace Travel_and_Accommodation_API.Services.DataAccess.Services
{
    public class CartBookingService : ICartBookingService
    {
        private readonly IRepository<CartBooking> _cartBookingRepository;
        private readonly ILogger<CartBookingService> _logger;
        private readonly IMapper _mapper;
        private readonly CartBookingValidation _cartBookingValidation;
        private readonly IRepository<CartBookingRoom> _cartBookingRoomRepository;
        public const int MaxCartSize = 5;

        public CartBookingService(
            IRepository<CartBooking> cartBookingRepository,
            ILogger<CartBookingService> logger,
            IMapper mapper,
            CartBookingValidation cartBookingValidation,
            IRepository<CartBookingRoom> cartBookingRoomRepository)
        {
            _cartBookingRepository = cartBookingRepository ?? throw new ArgumentNullException(nameof(cartBookingRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cartBookingValidation = cartBookingValidation ?? throw new ArgumentNullException(nameof(cartBookingValidation));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _cartBookingRoomRepository = cartBookingRoomRepository ?? throw new ArgumentNullException(nameof(cartBookingRoomRepository));
        }

        public async Task<IEnumerable<CartBookingDto>> GetAllAsync(int? pageSize, int? pageNumber)
        {
            try
            {
                var paging = new Paging(pageSize, pageNumber);
                var bookings = await _cartBookingRepository.GetAllAsync(paging);
                List< CartBookingDto> bookingsToBeReturned = _mapper.Map<List< CartBookingDto>>(bookings);
                Logs<CartBooking>.GetEntitiesLog(_logger, "CartBooking");
                return bookingsToBeReturned;
            }
            catch (Exception ex)
            {
                Logs<CartBooking>.GetEntitiesException(_logger, ex, "CartBooking");
                throw;
            }
        }

        public async Task< CartBookingDto> GetByIdAsync(Guid id, ClaimsPrincipal userClaims)
        {
            try
            {
                var booking = await GetBookingAsync(id);

                AuthorizeUser(userClaims, booking.UserId);
                var bookingToBeReturned = _mapper.Map< CartBookingDto>(booking);
                Logs<CartBooking>.GetEntityLog(_logger, "CartBooking", id);
                return bookingToBeReturned;
            }
            catch (ElementNotFoundException)
            {
                return null;
            }
            catch (Exception ex)
            {
                Logs<CartBooking>.GetEntityException(_logger, ex, "CartBooking", id);
                throw;
            }
        }

        public async Task<CartBooking> AddAsync(CartBookingToAdd bookingToAdd, ClaimsPrincipal userClaims)
        {
            try
            {
                AuthorizeUser(userClaims, bookingToAdd.UserId);
                var bookingToBeAdded = _mapper.Map<CartBooking>(bookingToAdd);
                bookingToBeAdded.CreationDate = DateTime.Now;

                await ValidateBooking(bookingToBeAdded);

                Expression<Func<CartBooking, bool>> filter = cb => cb.UserId == bookingToAdd.UserId;

                var carts = await _cartBookingRepository.GetFilteredItemsAsync(filter);

                if(carts != null && carts.Count() >= MaxCartSize)
                {
                    throw new ExceptionWithMessage("Max cart size has been reached");
                }
                var booking = await _cartBookingRepository.AddAsync(bookingToBeAdded);
                var cartBookingRoom = bookingToBeAdded.BookingRooms;
                cartBookingRoom.ForEach(bookingRoom => bookingRoom.CartBookingId = booking.Id);
                await _cartBookingRoomRepository.AddAllAsync(cartBookingRoom);
                Logs<CartBooking>.AddEntityLog(_logger, "CartBooking", booking);
                return booking;

            }
            catch (Exception ex)
            {
                Logs<CartBooking>.AddEntityException(_logger, ex, "CartBooking", _mapper.Map<CartBooking>(bookingToAdd));
                throw;
            }
        }

        public async Task UpdateAsync(Guid id, CartBookingDto bookingToUpdate, ClaimsPrincipal userClaims)
        {
            try
            {
                var oldBooking = await GetBookingAsync(id);

                AuthorizeUser(userClaims, oldBooking.UserId);

                var updatedBooking = oldBooking.Clone();
                _mapper.Map(bookingToUpdate, updatedBooking);
                await ValidateBooking(updatedBooking);
                await _cartBookingRepository.UpdateAsync(updatedBooking);
                Logs<CartBooking>.UpdateEntityLog(_logger, "CartBooking", oldBooking);
            }
            catch (Exception ex)
            {
                Logs<CartBooking>.UpdateEntityException(_logger, ex, "CartBooking", id);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id, ClaimsPrincipal userClaims)
        {
            try
            {
                var booking = await GetBookingAsync(id);
                AuthorizeUser(userClaims, booking.UserId);
                await _cartBookingRepository.DeleteAsync(id);
                Logs<CartBooking>.DeleteEntityLog(_logger, "CartBooking", booking);
            }
            catch (Exception ex)
            {
                Logs<CartBooking>.DeleteEntityException(_logger, ex, "CartBooking", id);
                throw;
            }
        }

        public async Task PartialUpdateAsync(Guid id, JsonPatchDocument<CartBooking> jsonPatchDocument, ClaimsPrincipal userClaims)
        {
            try
            {
                var booking = await GetBookingAsync(id);

                AuthorizeUser(userClaims, booking.UserId);

                var oldBooking = booking.Clone();

                jsonPatchDocument.ApplyTo(booking);
                await ValidateBooking(booking);

                await _cartBookingRepository.UpdateAsync(booking);
                Logs<CartBooking>.UpdateEntityLog(_logger, "CartBooking", oldBooking);
            }
            catch (Exception ex)
            {
                Logs<CartBooking>.UpdateEntityException(_logger, ex, "CartBooking", id);
                throw;
            }
        }

        public async Task<IEnumerable< CartBookingDto>> GetUserBookingsAsync(string userId,int? pageNumber,
            int? pageSize, ClaimsPrincipal userClaims)
        {
            try
            {
                AuthorizeUser(userClaims, userId);
                var expressions = new List<Expression<Func<CartBooking, bool>>>();
                var page = new Paging(pageSize, pageNumber);
                Expression<Func<CartBooking, bool>> ex = b => b.UserId == userId;
                expressions.Add(ex);

                var expression = new CustomExpression<CartBooking>
                {
                    Filter = ExpressionCombiner.CombineExpressions(expressions),
                    Paging = page
                };

                var bookings = await _cartBookingRepository.GetFilteredItemsAsync(expression);
                var bookingsToBeReturned = _mapper.Map<List<CartBookingDto>>(bookings);
                return bookingsToBeReturned;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"error while retrieving user {userId} CartBookings");
                throw;
            }
        }

        private async Task<CartBooking> GetBookingAsync(Guid id)
        {
            var booking = await _cartBookingRepository.GetByIdAsync(id);
            if (booking == null)
            {
                throw new ElementNotFoundException();
            }
            return booking;
        }

        private async Task ValidateBooking(CartBooking entity)
        {
            var validationResults = await _cartBookingValidation.ValidateAsync(entity);
            if (!validationResults.IsValid)
            {
                throw new ValidationException(validationResults);
            }
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
