using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Travel_and_Accommodation_API.Dto.Country;
using Travel_and_Accommodation_API.Exceptions_and_logs;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Services.DataAccess.IServices;

namespace Travel_and_Accommodation_API.Controllers
{
    [ApiController]
    [Route("/api/v1.0/countries")]
    [Authorize]
    public class CountryController : ControllerBase
    {
        private readonly ICountryService _countryService;
        private readonly ILogger<CountryController> _logger;

        public CountryController(
            ICountryService countryService,
            ILogger<CountryController> logger)
        {
            _countryService = countryService ?? throw new ArgumentNullException(nameof(countryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CountryDto>>> GetCountries(int? pageSize, int? pageNumber)
        {
            try
            {
                var countries = await _countryService.GetAllAsync(pageSize, pageNumber);

                return Ok(countries);
            }
            catch (Exception ex)
            {
                Logs<Country>.GetEntitiesException(_logger, ex, "Countries");
                throw;
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CountryDto>> GetCountryById(Guid id)
        {
            try
            {
                var country = await _countryService.GetByIdAsync(id);
                if (country == null)
                {
                    return NotFound();
                }
                return Ok(country);
            }
            catch (Exception ex)
            {
                Logs<Country>.GetEntityException(_logger, ex, "Country", id);
                throw;
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteCountry(Guid id)
        {
            try
            {
                await _countryService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                Logs<Country>.DeleteEntityException(_logger, ex, "Country", id);
                throw;
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateCountry(Guid id, [FromBody] CountryToAdd countryToUpdate)
        {
            try
            {
                if (countryToUpdate == null)
                {
                    return BadRequest();
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
                }
                await _countryService.UpdateAsync(id, countryToUpdate);
                return NoContent();
            }
            catch (Exception ex)
            {
                Logs<Country>.DeleteEntityException(_logger, ex, "Country", id);
                throw;
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AddCountry([FromBody] CountryToAdd country)
        {
            try
            {
                if (country == null)
                {
                    return BadRequest();
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
                }
                var addedCountry = await _countryService.AddAsync(country);

                return CreatedAtAction(nameof(GetCountries), new { id = addedCountry.Id }, null);
            }
            catch (Exception ex)
            {
                Logs<CountryToAdd>.AddEntityException(_logger, ex, "Country");
                throw;
            }
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> PartialUpdateCountry(Guid id, JsonPatchDocument<Country> jsonPatchDocument)
        {
            try
            {
                if (jsonPatchDocument == null)
                {
                    _logger.LogWarning("Invalid input provided for partial country update.");
                    return BadRequest();
                }

                await _countryService.PartialUpdateAsync(id, jsonPatchDocument);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while partially updating country.");
                throw;
            }
        }
    }
}
