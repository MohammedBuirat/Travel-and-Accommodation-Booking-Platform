using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Travel_and_Accommodation_API.Dto.City;
using Travel_and_Accommodation_API.Exceptions_and_logs;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Services.DataAccess.IServices;

namespace Travel_and_Accommodation_API.Controllers
{
    [ApiController]
    [Route("/api/v1.0/cities")]
    [Authorize]
    public class CityController : ControllerBase
    {
        private readonly ICityService _cityService;
        private readonly ILogger<CityController> _logger;

        public CityController(
            ICityService cityService,
            ILogger<CityController> logger)
        {
            _cityService = cityService ?? throw new ArgumentNullException(nameof(cityService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CityDto>>> GetCities(int? pageSize, int? pageNumber)
        {
            try
            {
                var cities = _cityService.GetAllAsync(pageSize, pageNumber);
                return Ok(cities);
            }
            catch (Exception ex)
            {
                Logs<City>.GetEntitiesException(_logger, ex, "Cities");
                throw;
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CityDto>> GetCityById(Guid id)
        {
            try
            {
                var city = await _cityService.GetByIdAsync(id);
                if (city == null)
                {
                    return NotFound();
                }
                return Ok(city);
            }
            catch (Exception ex)
            {
                Logs<City>.GetEntityException(_logger, ex, "City", id);
                throw;
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteCity(Guid id)
        {
            try
            {
                await _cityService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                Logs<City>.DeleteEntityException(_logger, ex, "City", id);
                throw;
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateCity(Guid id, [FromBody] CityToAdd cityToUpdate)
        {
            try
            {
                if (cityToUpdate == null)
                {
                    return BadRequest();
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
                }
                await _cityService.UpdateAsync(id, cityToUpdate);

                return NoContent();
            }
            catch (Exception ex)
            {
                Logs<City>.DeleteEntityException(_logger, ex, "City", id);
                throw;
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AddCity([FromBody] CityToAdd city)
        {
            try
            {
                if (city == null)
                {
                    return BadRequest();
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
                }
                var newCity = await _cityService.AddAsync(city);
                return CreatedAtAction(nameof(GetCities), new { id = newCity.Id }, null);
            }
            catch (Exception ex)
            {
                Logs<CityToAdd>.AddEntityException(_logger, ex, "City");
                throw;
            }
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> PartialUpdateCity(Guid id, JsonPatchDocument<City> jsonPatchDocument)
        {
            try
            {
                if (jsonPatchDocument == null)
                {
                    _logger.LogWarning("Invalid input provided for partial city update.");
                    return BadRequest();
                }

                await _cityService.PartialUpdateAsync(id, jsonPatchDocument);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while partially updating city.");
                throw;
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<CityDto>>> SearchCities(string name, int? pageSize, int? pageNumber)
        {
            try
            {
                var cities = await _cityService.SearchCitiesAsync(name, pageSize, pageNumber);
                return Ok(cities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching for cities.");
                throw;
            }
        }

        [HttpGet("admin-search")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<CityDto>>> AdminSearchCities(
            string? name, int? pageSize, int? pageNumber, Guid? country, DateTimeOffset? creationDate)
        {
            try
            {
                var cities = await _cityService.AdminCitiesSearchAsync(name, pageSize, pageNumber, country, creationDate);
                return Ok(cities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching for cities.");
                throw;
            }
        }

        [HttpGet("top-cities")]
        public async Task<ActionResult<IEnumerable<CityDto>>> GetTopCities(int? pageSize, int? pageNumber)
        {
            try
            {
                var cities = await _cityService.MostVisitedCitiesAsync(pageSize, pageNumber);
                return Ok(cities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving most visited cities.");
                throw;
            }
        }
    }
}
