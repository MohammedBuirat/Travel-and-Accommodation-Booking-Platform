using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Travel_and_Accommodation_API.Dto.Booking;
using Travel_and_Accommodation_API.Exceptions_and_logs;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Services.DataAccess.IServices;

namespace Travel_and_Accommodation_API.Controllers
{
    [ApiController]
    [Route("api/v1.0/")]
    [Authorize]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<BookingController> _logger;

        public BookingController(IBookingService bookingService,
            ILogger<BookingController> logger)
        {
            _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("bookings")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<BookingDto>>> GetAllBookings(int? pageSize, int? pageNumber)
        {
            try
            {
                var bookings = await _bookingService.GetAllAsync(pageSize, pageNumber);
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                Logs<Booking>.GetEntitiesException(_logger, ex, "Bookings");
                throw;
            }
        }

        [HttpGet("bookings/{id}")]
        public async Task<ActionResult<BookingDto>> GetBookingById(Guid id)
        {
            try
            {
                var bookings = await _bookingService.GetByIdAsync(id, User);
                if (bookings == null)
                {
                    return NotFound();
                }
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                Logs<Booking>.GetEntityException(_logger, ex, "Booking", id);
                throw;
            }
        }

        [HttpDelete("bookings/{id}")]
        public async Task<ActionResult> DeleteBooking(Guid id)
        {
            try
            {
                await _bookingService.DeleteAsync(id, User);
                return NoContent();
            }
            catch (Exception ex)
            {
                Logs<Booking>.DeleteEntityException(_logger, ex, "Booking", id);
                throw;
            }
        }

        [HttpPut("bookings/{id}")]
        public async Task<ActionResult> UpdateBooking(Guid id, [FromBody] BookingDto bookingToUpdate)
        {
            try
            {
                if (bookingToUpdate == null)
                {
                    return BadRequest();
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
                }
                await _bookingService.UpdateAsync(id, bookingToUpdate, User);
                return NoContent();
            }
            catch (Exception ex)
            {
                Logs<Booking>.UpdateEntityException(_logger, ex, "Booking", id);
                throw;
            }
        }

        [HttpPost("bookings")]
        public async Task<ActionResult> AddBooking([FromBody] BookingToAdd bookingToAdd)
        {
            try
            {
                if (bookingToAdd == null)
                {
                    return BadRequest();
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
                }
                var booking = await _bookingService.AddAsync(bookingToAdd, User);
                return CreatedAtAction(nameof(GetAllBookings), new { id = booking.Id }, null);
            }
            catch (Exception ex)
            {
                Logs<Booking>.AddEntityException(_logger, ex, "Booking");
                throw;
            }
        }

        [HttpPatch("bookings/{id}")]
        public async Task<ActionResult> PartialUpdateBooking(Guid id, JsonPatchDocument<Booking> jsonPatchDocument)
        {
            try
            {
                if (jsonPatchDocument == null)
                {
                    return BadRequest();
                }
                await _bookingService.PartialUpdateAsync(id, jsonPatchDocument, User);
                return Ok();
            }
            catch (Exception ex)
            {
                Logs<Booking>.UpdateEntityException(_logger, ex, "Booking", id);
                throw;
            }
        }

        [HttpGet("users/{userId}/bookings")]
        public async Task<ActionResult<IEnumerable<BookingDto>>> GetUserBookings(string userId,
            bool? previous, bool? upComing, int? pageNumber, int? pageSize)
        {
            try
            {
                var bookings = await _bookingService.GetUserBookings(userId,
                previous, upComing, pageNumber, pageSize, User);
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing the Get user bookings request.");
                throw;
            }
        }
    }
}
