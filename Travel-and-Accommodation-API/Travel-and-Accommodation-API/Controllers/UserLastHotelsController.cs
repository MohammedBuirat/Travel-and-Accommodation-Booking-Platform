using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Travel_and_Accommodation_API.Dto.UserLastHotels;
using Travel_and_Accommodation_API.Exceptions_and_logs;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Services.DataAccess.IServices;

namespace Travel_and_Accommodation_API.Controllers
{
    [ApiController]
    [Route("/api/v1.0/")]
    [Authorize]
    public class UserLastHotelsController : ControllerBase
    {
        private readonly IUserLastHotelsService _userLastHotelsService;
        private readonly ILogger<UserLastHotelsController> _logger;

        public UserLastHotelsController(IUserLastHotelsService userLastHotelsService, ILogger<UserLastHotelsController> logger)
        {
            _userLastHotelsService = userLastHotelsService ?? throw new ArgumentNullException(nameof(userLastHotelsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("user/{userId}/lastHotels")]
        public async Task<ActionResult<IEnumerable<UserLastHotelsDto>>> GetUserLastHotels(string userId)
        {
            try
            {
                var userLastHotels = await _userLastHotelsService.GetUsersLastHotelsAsync(userId, User);

                return Ok(userLastHotels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while retrieving {userId} last hotels");
                throw;
            }
        }

        [HttpDelete("{userId}/lastHotels/{hotelId}")]
        public async Task<ActionResult> Delete(Guid hotelId, string userId)
        {
            try
            {
                await _userLastHotelsService.DeleteAsync(hotelId, userId, User);
                return NoContent();
            }
            catch (Exception ex)
            {
                Logs<UserLastHotels>.DeleteEntityException(_logger, ex, "UserLastHotels", Guid.Parse(userId), hotelId);
                throw;
            }
        }
    }
}
