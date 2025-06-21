using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Travel_and_Accommodation_API.Dto.CartBookingRoom;
using Travel_and_Accommodation_API.Exceptions_and_logs;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Services.DataAccess.IServices;

namespace Travel_and_Accommodation_API.Controllers
{
    [ApiController]
    [Route("/api/v1.0/")]
    [Authorize]
    public class CartBookingRoomController : ControllerBase
    {
        private readonly ICartBookingRoomService _cartBookingRoomService;
        private readonly ILogger<CartBookingRoomController> _logger;

        public CartBookingRoomController(ICartBookingRoomService cartBookingRoomService,
            ILogger<CartBookingRoomController> logger)
        {
            _cartBookingRoomService = cartBookingRoomService ?? throw new ArgumentNullException(nameof(cartBookingRoomService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpDelete("cartBookingRooms/{roomId}/{cartBookingId}")]
        public async Task<ActionResult> DeleteCartBookingRoom(Guid roomId, Guid cartBookingId)
        {
            try
            {
                await _cartBookingRoomService.DeleteAsync(bookingId: cartBookingId,
                    roomId: roomId, User);
                return NoContent();
            }
            catch (Exception ex)
            {
                Logs<CartBookingRoom>.DeleteEntityException(_logger, ex, "CartBookingRoom", cartBookingId, roomId);
                throw;
            }
        }

        [HttpPost("cartBookingRooms")]
        public async Task<ActionResult> AddCartBookingRoom([FromBody] CartBookingRoomDto newCartBookingRoomDto)
        {
            try
            {
                if (newCartBookingRoomDto == null)
                {
                    return BadRequest("Invalid Request Body");
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
                }
                var cartRoom = await _cartBookingRoomService.AddAsync(newCartBookingRoomDto, User);
                return StatusCode(201, new { message = "Cart Booking Room created successfully" });
            }
            catch (Exception ex)
            {
                Logs<CartBookingRoom>.AddEntityException(_logger, ex, "CartBookingRoom");
                throw;
            }
        }

        [HttpGet("cartBooking/{bookingId}/bookingRooms")]
        public async Task<ActionResult<IEnumerable<CartBookingRoomDto>>> GetBookingRoomsByBookingId(Guid bookingId, int? pageSize, int? pageNumber)
        {
            try
            {
                var bookingRooms = await _cartBookingRoomService.GetBookingRoomsByBookingIdAsync(bookingId, pageSize, pageNumber, User);
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
