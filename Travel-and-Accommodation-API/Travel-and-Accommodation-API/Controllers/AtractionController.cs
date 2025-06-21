using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Travel_and_Accommodation_API.Dto.Attraction;
using Travel_and_Accommodation_API.Exceptions_and_logs;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Services.DataAccess.IServices;
using Travel_and_Accommodation_API.Services.Validation;

namespace Travel_and_Accommodation_API.Controllers
{
    [ApiController]
    [Route("/api/v1.0/")]
    [Authorize]
    public class AttractionController : ControllerBase
    {
        private readonly IAttractionService _attractionService;
        private readonly ILogger<AttractionController> _logger;

        public AttractionController(IAttractionService attractionService, ILogger<AttractionController> logger)
        {
            _attractionService = attractionService ?? throw new ArgumentNullException(nameof(attractionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("attractions")]
        public async Task<ActionResult<IEnumerable<AttractionDto>>> GetAttractions(int? pageSize, int? pageNumber)
        {
            try
            {
                var attractions = await _attractionService.GetAllAsync(pageSize, pageNumber);
                return Ok(attractions);
            }
            catch (Exception ex)
            {
                Logs<Attraction>.GetEntitiesException(_logger, ex, "Attractions");
                throw;
            }
        }

        [HttpGet("attractions/{id}")]
        public async Task<ActionResult<AttractionDto>> GetAttractionById(Guid id)
        {
            try
            {
                var attraction = await _attractionService.GetByIdAsync(id);
                if(attraction == null)
                {
                    return NotFound();
                }
                return attraction;
            }
            catch (Exception ex)
            {
                Logs<Attraction>.GetEntityException(_logger, ex, "Attraction", id);
                throw;
            }
        }

        [HttpDelete("attractions/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteAttraction(Guid id)
        {
            try
            {
                await _attractionService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                Logs<Attraction>.DeleteEntityException(_logger, ex, "Attraction", id);
                throw;
            }
        }

        [HttpPut("attractions/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateAttraction(Guid id, [FromBody] AttractionToAdd updatedAttractionDto)
        {
            try
            {

                if (updatedAttractionDto == null)
                {
                    return BadRequest();
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
                }
                await _attractionService.UpdateAsync(id, updatedAttractionDto);
                return NoContent();
            }
            catch (Exception ex)
            {
                Logs<Attraction>.DeleteEntityException(_logger, ex, "Attraction", id);
                throw;
            }
        }

        [HttpPatch("attractions/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> PatchAttraction(Guid id, [FromBody] JsonPatchDocument<Attraction> patchDocument)
        {
            try
            {
                if (patchDocument == null)
                {
                    return BadRequest();
                }

                await _attractionService.PartialUpdateAsync(id, patchDocument);
                return NoContent();
            }
            catch (Exception ex)
            {
                Logs<Attraction>.UpdateEntityException(_logger, ex, "Attraction", id);
                throw;
            }
        }

        [HttpPost("attractions")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AddAttraction([FromBody] AttractionToAdd attractionToAdd)
        {
            try
            {
                if (attractionToAdd == null)
                {
                    return BadRequest();
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
                }
                var attraction = await _attractionService.AddAsync(attractionToAdd);
                return CreatedAtAction(nameof(GetAttractions), new { id = attraction.Id }, null);
            }
            catch (Exception ex)
            {
                Logs<AttractionToAdd>.AddEntityException(_logger, ex, "Attraction");
                throw;
            }
        }

        [HttpGet("cities/{id}/attractions")]
        public async Task<ActionResult<IEnumerable<AttractionDto>>> GetCityAttractions(Guid cityId, int? pageSize, int? pageNumber)
        {
            try
            {
                var attractions = await _attractionService.GetCityAttractionsAsync(cityId, pageSize, pageNumber);
                return Ok(attractions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing Get attraction by city id request.");
                throw;
            }
        }
    }
}
