using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Travel_and_Accommodation_API.Dto.HotelImage;
using Travel_and_Accommodation_API.Exceptions_and_logs;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Services.DataAccess.IServices;

namespace Travel_and_Accommodation_API.Controllers
{
    [ApiController]
    [Route("/api/v1.0/")]
    [Authorize]
    public class HotelImageController : Controller
    {
        private readonly IHotelImageService _hotelImageService;
        private readonly ILogger<HotelImageController> _logger;

        public HotelImageController(
            IHotelImageService hotelImageService,
            ILogger<HotelImageController> logger)
        {
            _hotelImageService = hotelImageService ?? throw new ArgumentNullException(nameof(hotelImageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("hotelImages")]
        public async Task<ActionResult<IEnumerable<HotelImageDto>>> GetHotelImages(int? pageSize, int? pageNumber)
        {
            try
            {
                var images = await _hotelImageService.GetAllAsync(pageSize, pageNumber);
                return Ok(images);
            }
            catch (Exception ex)
            {
                Logs<HotelImage>.GetEntitiesException(_logger, ex, "HotelImages");
                throw;
            }
        }

        [HttpPost("hotelImages")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AddHotelImage([FromBody] HotelImageToAdd hotelImage)
        {
            try
            {
                if (hotelImage == null)
                {
                    return BadRequest();
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
                }
                await _hotelImageService.AddAsync(hotelImage);
                return NoContent();
            }
            catch (Exception ex)
            {
                Logs<HotelImageToAdd>.AddEntityException(_logger, ex, "HotelImage");
                throw;
            }
        }

        [HttpDelete("hotelImages")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteHotelImage(Guid hotelId, string imagePath)
        {
            try
            {
                await _hotelImageService.DeleteAsync(hotelId, imagePath);
                return NoContent();
            }
            catch (Exception ex)
            {
                Logs<HotelImage>.DeleteEntityException(_logger, ex, "HotelImage", hotelId);
                throw;
            }
        }

        [HttpGet("hotels/{hotelId}/hotelImages")]
        public async Task<ActionResult<IEnumerable<HotelImageDto>>> GetHotelImagesByHotelId(Guid hotelId)
        {
            try
            {
                var image = await _hotelImageService.GetByHotelIdAsync(hotelId);
                return Ok(image);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching hotel images: {ex.Message}");
                throw;
            }
        }

        [HttpGet("hotels/{hotelId}/{imagePath}")]
        public async Task<ActionResult<IEnumerable<HotelImageDto>>> GetHotelImage(Guid hotelId, string imagePath)
        {
            try
            {
                var image = await _hotelImageService.GetAsync(hotelId, imagePath);
                if (image == null)
                {
                    return NotFound();
                }
                return Ok(image);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching hotel image: {ex.Message}");
                throw;
            }
        }
    }
}
