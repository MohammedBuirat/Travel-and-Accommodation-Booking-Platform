using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using Travel_and_Accommodation_API.DataAccess.Repositories;
using Travel_and_Accommodation_API.DataAccess.Repositories.IRepository;
using Travel_and_Accommodation_API.DataAccess.UnitOfWork;
using Travel_and_Accommodation_API.DataAccess.UnitOfWork.IUnitOfWorks;
using Travel_and_Accommodation_API.Dto.Booking;
using Travel_and_Accommodation_API.Exceptions_and_logs;
using Travel_and_Accommodation_API.Helpers;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Services.DataAccess.IServices;
using Travel_and_Accommodation_API.Services.Validation;

namespace Travel_and_Accommodation_API.Services.DataAccess.Services
{
    public class BookingService : IBookingService
    {
        private readonly IRepository<Booking> _bookingRepository;
        private readonly ILogger<BookingService> _logger;
        private readonly IBookingUnitOfWork _bookingUnitOfWork;
        private readonly IMapper _mapper;
        private readonly BookingValidation _bookingValidation;

        public BookingService(
            IRepository<Booking> bookingRepository,
            ILogger<BookingService> logger,
            IBookingUnitOfWork bookingUnitOfWork,
            IMapper mapper,
            BookingValidation bookingValidation)
        {
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _bookingUnitOfWork = bookingUnitOfWork ?? throw new ArgumentNullException(nameof(bookingUnitOfWork));
            _bookingValidation = bookingValidation ?? throw new ArgumentNullException(nameof(bookingValidation));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<BookingDto>> GetAllAsync(int? pageSize, int? pageNumber)
        {
            try
            {
                var paging = new Paging(pageSize, pageNumber);
                IEnumerable<Booking> bookings = await _bookingRepository.GetAllAsync(paging);
                List<BookingDto> bookingsToBeReturned = _mapper.Map<List<BookingDto>>(bookings);
                Logs<Booking>.GetEntitiesLog(_logger, "Bookings");
                return bookingsToBeReturned;
            }
            catch (Exception ex)
            {
                Logs<Booking>.GetEntitiesException(_logger, ex, "Bookings");
                throw;
            }
        }

        public async Task<BookingDto> GetByIdAsync(Guid id, ClaimsPrincipal userClaims)
        {
            try
            {
                var booking = await GetBookingAsync(id);

                AuthorizeUser(userClaims, booking.UserId);
                var bookingToBeReturned = _mapper.Map<BookingDto>(booking);
                Logs<Booking>.GetEntityLog(_logger, "Booking", id);
                return bookingToBeReturned;
            }
            catch (ElementNotFoundException)
            {
                return null;
            }
            catch (Exception ex)
            {
                Logs<Booking>.GetEntityException(_logger, ex, "Booking", id);
                throw;
            }
        }

        public async Task<Booking> AddAsync(BookingToAdd bookingToAdd, ClaimsPrincipal userClaims)
        {
            try
            {
                AuthorizeUser(userClaims, bookingToAdd.UserId);
                var bookingToBeAdded = _mapper.Map<Booking>(bookingToAdd);
                bookingToBeAdded.BookingDate = DateTime.Now;
                int randomConfirmationNumber = new Random().Next(100000000, 999999999);
                bookingToBeAdded.ConfirmationNumber = randomConfirmationNumber;

                await ValidateBooking(bookingToBeAdded);
                var booking = await _bookingUnitOfWork.AddBookingAsync(bookingToBeAdded);
                Logs<Booking>.AddEntityLog(_logger, "Booking", booking);
                return booking;

            }
            catch (Exception ex)
            {
                Logs<Booking>.AddEntityException(_logger, ex, "Booking", _mapper.Map<Booking>(bookingToAdd));
                throw;
            }
        }

        public async Task UpdateAsync(Guid id, BookingDto bookingToUpdate, ClaimsPrincipal userClaims)
        {
            try
            {
                var oldBooking = await GetBookingAsync(id);

                AuthorizeUser(userClaims, oldBooking.UserId);

                var updatedBooking = oldBooking.Clone();
                _mapper.Map(bookingToUpdate, updatedBooking);
                await ValidateBooking(updatedBooking);
                await _bookingRepository.UpdateAsync(updatedBooking);
                Logs<Booking>.UpdateEntityLog(_logger, "Booking", oldBooking);
            }
            catch (Exception ex)
            {
                Logs<Booking>.UpdateEntityException(_logger, ex, "Booking", id);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id, ClaimsPrincipal userClaims)
        {
            try
            {
                var booking = await GetBookingAsync(id);
                AuthorizeUser(userClaims, booking.UserId);
                await _bookingUnitOfWork.DeleteBookingAsync(id);
                Logs<Booking>.DeleteEntityLog(_logger, "Booking", booking);
            }
            catch (Exception ex)
            {
                Logs<Booking>.DeleteEntityException(_logger, ex, "Booking", id);
                throw;
            }
        }

        public async Task PartialUpdateAsync(Guid id, JsonPatchDocument<Booking> jsonPatchDocument, ClaimsPrincipal userClaims)
        {
            try
            {
                var booking = await GetBookingAsync(id);

                AuthorizeUser(userClaims, booking.UserId);

                var oldBooking = booking.Clone();

                jsonPatchDocument.ApplyTo(booking);
                await ValidateBooking(booking);

                await _bookingRepository.UpdateAsync(booking);
                Logs<Booking>.UpdateEntityLog(_logger, "Booking", oldBooking);
            }
            catch (Exception ex)
            {
                Logs<Booking>.UpdateEntityException(_logger, ex, "Booking", id);
                throw;
            }
        }

        public async Task<IEnumerable<BookingDto>> GetUserBookings(string userId, bool? previous, bool? upComing, int? pageNumber,
            int? pageSize, ClaimsPrincipal userClaims)
        {
            try
            {
                AuthorizeUser(userClaims, userId);
                var expressions = new List<Expression<Func<Booking, bool>>>();
                if (!(previous != null && upComing != null))
                {
                    if (previous != null && previous == true)
                    {
                        Expression<Func<Booking, bool>> temp = b => b.CheckInDate < DateTime.Now;
                        expressions.Add(temp);
                    }
                    if (upComing != null && upComing == true)
                    {
                        Expression<Func<Booking, bool>> temp = b => b.CheckInDate > DateTime.Now;
                        expressions.Add(temp);
                    }
                }
                var page = new Paging(pageSize, pageNumber);
                Expression<Func<Booking, bool>> ex = b => b.UserId == userId;
                expressions.Add(ex);

                var expression = new CustomExpression<Booking>
                {
                    Filter = ExpressionCombiner.CombineExpressions(expressions),
                    Paging = page
                };

                var bookings = await _bookingRepository.GetFilteredItemsAsync(expression);
                var bookingsToBeReturned = _mapper.Map<List<BookingDto>>(bookings);
                return bookingsToBeReturned;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"error while retrieving user {userId} bookings");
                throw;
            }
        }

        private async Task<Booking> GetBookingAsync(Guid id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
            {
                throw new ElementNotFoundException();
            }
            return booking;
        }

        private async Task ValidateBooking(Booking entity)
        {
            var validationResults = await _bookingValidation.ValidateAsync(entity);
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
