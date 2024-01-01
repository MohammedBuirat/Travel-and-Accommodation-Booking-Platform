using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Travel_and_Accommodation_API.Dto.BookingRoom;
using Travel_and_Accommodation_API.Exceptions_and_logs;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Services.DataAccess.IServices;

namespace Travel_and_Accommodation_API.Controllers
{
    [ApiController]
    [Route("/api/v1.0/")]
    [Authorize]
    public class BookingRoomController : ControllerBase
    {
        private readonly IBookingRoomService _bookingRoomService;
        private readonly ILogger<BookingRoomController> _logger;

        public BookingRoomController(IBookingRoomService bookingRoomService,
            ILogger<BookingRoomController> logger)
        {
            _bookingRoomService = bookingRoomService ?? throw new ArgumentNullException(nameof(bookingRoomService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpDelete("bookingRooms/{bookingId}/{roomId}")]
        public async Task<ActionResult> DeleteBookingRoom(Guid bookingId, Guid roomId)
        {
            try
            {
                await _bookingRoomService.DeleteAsync(bookingId, roomId, User);
                return NoContent();
            }
            catch (Exception ex)
            {
                Logs<BookingRoom>.DeleteEntityException(_logger, ex, "BookingRoom", bookingId, roomId);
                throw;
            }
        }

        [HttpPost("bookingRooms")]
        public async Task<ActionResult> AddBookingRoom([FromBody] BookingRoomDto newBookingRoomDto)
        {
            try
            {
                if (newBookingRoomDto == null)
                {
                    return BadRequest();
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
                }
                var booking = await _bookingRoomService.AddAsync(newBookingRoomDto, User);
                return StatusCode(201, new { message = "Booking room created successfully" });
            }
            catch (Exception ex)
            {
                Logs<BookingRoom>.AddEntityException(_logger, ex, "BookingRoom");
                throw;
            }
        }

        [HttpGet("booking/{bookingId}/bookingRooms")]
        public async Task<ActionResult<IEnumerable<BookingRoomDto>>> GetBookingRoomsByBookingId(Guid bookingId, int? pageSize, int? pageNumber)
        {
            try
            {
                var bookingRooms = await _bookingRoomService.GetBookingRoomsByBookingIdAsync(bookingId, pageSize, pageNumber, User);
                return Ok(bookingRooms);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing the request in GetBookingRoomsByBookingId method.");
                throw;
            }
        }

        [HttpGet("rooms/{roomId}/bookingRooms")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<BookingRoomDto>>> GetBookingRoomsByRoomId(Guid roomId, int? pageSize, int? pageNumber)
        {
            try
            {
                var bookingRooms = await _bookingRoomService.GetBookingRoomsByRoomIdIdAsync(roomId, pageSize, pageNumber, User);
                return Ok(bookingRooms);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing the request in GetBookingRoomsByBookingId method.");
                throw;
            }
        }

    }
}
