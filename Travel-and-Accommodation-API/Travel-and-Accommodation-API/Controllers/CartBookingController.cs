using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Travel_and_Accommodation_API.Dto.CartBooking;
using Travel_and_Accommodation_API.Exceptions_and_logs;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Services.DataAccess.IServices;

namespace Travel_and_Accommodation_API.Controllers
{
    [ApiController]
    [Route("/api/v1.0/")]
    [Authorize]
    public class CartBookingController : ControllerBase
    {
        private readonly ICartBookingService _cartBookingService;
        private readonly ILogger<CartBookingController> _logger;

        public CartBookingController(
            ICartBookingService cartBookingService,
            ILogger<CartBookingController> logger)
        {
            _cartBookingService = cartBookingService ?? throw new ArgumentNullException(nameof(cartBookingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("cartBookings")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<CartBookingDto>>> GetCartBookings(int? pageSize, int? pageNumber)
        {
            try
            {
                var cartBookings = await _cartBookingService.GetAllAsync(pageSize, pageNumber);
                return Ok(cartBookings);
            }
            catch (Exception ex)
            {
                Logs<CartBooking>.GetEntitiesException(_logger, ex, "CartBooking");
                throw;
            }
        }

        [HttpGet("cartBookings/{id}")]
        public async Task<ActionResult<CartBookingDto>> GetCartBookingById(Guid id)
        {
            try
            {
                var cartBooking = await _cartBookingService.GetByIdAsync(id, User);
                if (cartBooking == null)
                {
                    return NotFound();
                }
                return Ok(cartBooking);
            }
            catch (Exception ex)
            {
                Logs<CartBooking>.GetEntityException(_logger, ex, "CartBooking", id);
                throw;
            }
        }

        [HttpDelete("cartBookings/{id}")]
        public async Task<ActionResult> DeleteCartBooking(Guid id)
        {
            try
            {
                await _cartBookingService.DeleteAsync(id, User);
                return NoContent();
            }
            catch (Exception ex)
            {
                Logs<CartBooking>.DeleteEntityException(_logger, ex, "CartBooking", id);
                throw;
            }
        }

        [HttpPut("cartBookings/{id}")]
        public async Task<ActionResult> UpdateCartBooking(Guid id, [FromBody] CartBookingDto updatedCartBooking)
        {
            try
            {
                if (updatedCartBooking == null)
                {
                    return BadRequest();
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
                }
                await _cartBookingService.UpdateAsync(id, updatedCartBooking, User);
                return NoContent();
            }
            catch (Exception ex)
            {
                Logs<CartBooking>.UpdateEntityException(_logger, ex, "CartBooking", id);
                throw;
            }
        }

        [HttpPost("cartBookings")]
        public async Task<ActionResult> AddCartBooking([FromBody] CartBookingToAdd cartBookingToAdd)
        {
            try
            {
                if (cartBookingToAdd == null)
                {
                    return BadRequest();
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
                }
                var cart = await _cartBookingService.AddAsync(cartBookingToAdd, User);
                return CreatedAtAction(nameof(GetCartBookingById), new { id = cart.Id }, null);
            }
            catch (Exception ex)
            {
                Logs<CartBooking>.AddEntityException(_logger, ex, "CartBooking");
                throw;            }
        }

        [HttpPatch("cartBookings/{id}")]
        public async Task<ActionResult> PartialUpdateCartBooking(Guid id, JsonPatchDocument<CartBooking> jsonPatchDocument)
        {
            try
            {
                if (jsonPatchDocument == null)
                {
                    return BadRequest();
                }

                await _cartBookingService.PartialUpdateAsync(id, jsonPatchDocument, User);
                return NoContent();
            }
            catch (Exception ex)
            {
                Logs<CartBooking>.UpdateEntityException(_logger, ex, "CartBookingRoom", id);
                throw;            }
        }

        [HttpGet("users/{userId}/cartBookings")]
        public async Task<ActionResult<IEnumerable<CartBookingDto>>> GetUserCartBookings(string userId, int? pageSize, int? pageNumber)
        {
            try
            {
                var cartBooking = await _cartBookingService.GetUserBookingsAsync(userId, pageSize, pageNumber, User);
                return Ok(cartBooking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing the request in GetUserCartBookings method.");
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
