using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Travel_and_Accommodation_API.Dto.Hotel;
using Travel_and_Accommodation_API.Helpers;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Services.DataAccess.IServices;
using static Travel_and_Accommodation_API.Models.Enums;
using Travel_and_Accommodation_API.Views;
using Travel_and_Accommodation_API.Exceptions_and_logs;

namespace Travel_and_Accommodation_API.Controllers
{
    [ApiController]
    [Route("/api/v1.0/")]
    //[Authorize]
    public class HotelController : ControllerBase
    {
        private readonly IHotelService _hotelService;
        private readonly ILogger<HotelController> _logger;

        public HotelController(IHotelService hotelService, ILogger<HotelController> logger)
        {
            _hotelService = hotelService ?? throw new ArgumentNullException(nameof(hotelService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("hotels")]
        public async Task<ActionResult<IEnumerable<HotelDto>>> GetAllHotels(int? pageSize, int? pageNumber)
        {
            try
            {
                var hotels = await _hotelService.GetAllAsync(pageSize, pageNumber);
                return Ok(hotels);
            }
            catch (Exception ex)
            {
                Logs<Hotel>.GetEntitiesException(_logger, ex, "Hotels");
                throw;
            }
        }

        [HttpGet("hotels/{id}")]
        public async Task<ActionResult<HotelDto>> GetHotelById(Guid id)
        {
            try
            {
                var hotel = await _hotelService.GetByIdAsync(id, User);
                return Ok(hotel);
            }
            catch (Exception ex)
            {
                Logs<Hotel>.GetEntityException(_logger, ex, "Hotel", id);
                throw;
            }
        }

        [HttpDelete("hotels/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteHotel(Guid id)
        {
            try
            {
                await _hotelService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                Logs<Hotel>.DeleteEntityException(_logger, ex, "Hotel", id);
                throw;
            }
        }

        [HttpPut("hotels/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateHotel(Guid id, [FromBody] HotelToAdd hotelToUpdate)
        {
            try
            {
                if (hotelToUpdate == null)
                {
                    return BadRequest();
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
                }
                await _hotelService.UpdateAsync(id, hotelToUpdate);
                return NoContent();
            }
            catch (Exception ex)
            {
                Logs<Hotel>.DeleteEntityException(_logger, ex, "Hotel", id);
                throw;
            }
        }

        [HttpPost("hotels")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AddHotel([FromBody] HotelToAdd hotel)
        {
            try
            {
                if (hotel == null)
                {
                    return BadRequest();
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
                }
                var newHotel = await _hotelService.AddAsync(hotel);
                return CreatedAtAction(nameof(GetAllHotels), new { id = newHotel.Id }, null);
            }
            catch (Exception ex)
            {
                Logs<HotelToAdd>.AddEntityException(_logger, ex, "Hotel");
                throw;
            }
        }

        [HttpPatch("hotels/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> PartialUpdateHotel(Guid id, JsonPatchDocument<Hotel> jsonPatchDocument)
        {
            try
            {
                if (jsonPatchDocument == null)
                {
                    _logger.LogWarning("Invalid input provided for partial hotel update.");
                    return BadRequest();
                }
                await _hotelService.PartialUpdateAsync(id, jsonPatchDocument);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing the PartialUpdate hotel request.");
                throw;
            }
        }

        [HttpGet("cities/{cityId}/hotels")]
        public async Task<ActionResult<IEnumerable<HotelDto>>> GetCityHotels(Guid cityId, int? pageSize, int? pageNum)
        {
            try
            {
                var hotels = await _hotelService.GetCityHotelsAsync(cityId, pageSize, pageNum);
                return Ok(hotels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing the GetCityHotels request.");
                throw;
            }
        }

        [HttpGet("hotels/admin-search")]
        public async Task<ActionResult<IEnumerable<HotelDto>>> AdminSearchHotels([FromQuery] string? name, int? pageSize, int? pageNumber,
            Guid? cityId, DateTimeOffset? creationDate, decimal? ratings)
        {
            try
            {
                var hotels = await _hotelService.AdminSearchHotelsAsync(name, pageSize, pageNumber, cityId, creationDate, ratings);
                return Ok(hotels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching for hotels.");
                throw;
            }
        }

        [HttpGet("hotels/discountedHotels")]
        public async Task<ActionResult<IEnumerable<HotelDto>>> GetDiscountedHotelRooms(int? pageSize, int? pageNumber)
        {
            try
            {
                var hotels = await _hotelService.GetDiscountedHotelRoomsAsync(pageSize, pageNumber);
                return Ok(hotels);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetDiscountedHotelRooms: {ex.Message}");
                throw;
            }
        }

        [HttpPost("hotels-filter/{cityId}")]
        public async Task<IActionResult> GetSearchedFilteredHotels(Guid cityId, int numberOfAdults, int numberOfChildren,
            DateTime checkInDate, DateTime checkOutDate, int? pageSize, int? pageNumber, [FromBody] List<string>? amenities,
            SortCriteria? sort, decimal? maxPrice, decimal? minPrice, int? minRatings, RoomType? roomType,
            bool? descendingOrder, decimal? distanceFromCityCenter)
        {
            try
            {
                if ((numberOfAdults + numberOfChildren) == 0)
                    return BadRequest("Number of adults and children are required.");

                if (checkInDate >= checkOutDate)
                {
                    return BadRequest("Check in date should be less than check out date");
                }
                var hotels = await _hotelService.GetSearchedFilteredHotelsAsync(cityId, numberOfAdults, numberOfChildren,
                    checkInDate, checkOutDate, pageSize, pageNumber, amenities, sort
                    , maxPrice, minPrice, minRatings, roomType, descendingOrder, distanceFromCityCenter);
                return Ok(hotels);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetSearchedFilteredHotels: {ex.Message}");
                throw;
            }
        }
    }
}
