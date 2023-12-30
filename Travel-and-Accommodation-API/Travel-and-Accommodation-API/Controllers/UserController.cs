using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Travel_and_Accommodation_API.Dto.User;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Services.DataAccess.IServices;

namespace Travel_and_Accommodation_API.Controllers
{
    [ApiController]
    [Route("/api/v1.0/users")]
    [Authorize]
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserService _userSerivce;

        public UserController(ILogger<UserController> logger, IUserService userService)
        {
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
            _userSerivce = userService ??
                throw new ArgumentNullException(nameof(userService));

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUserById(string id)
        {
            try
            {
                var user = await _userSerivce.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while fetching user with ID: {id}");
                throw;
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(string id)
        {
            try
            {
                await _userSerivce.DeleteAsync(id, User);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while deleting user with ID: {id}");
                throw;
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateUser(string id, [FromBody] User userToUpdate)
        {
            try
            {
                await _userSerivce.UpdateAsync(id, userToUpdate, User);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while updating user with ID: {id}");
                throw;
            }
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> PartialUpdateUser(string id, JsonPatchDocument<User> jsonPatchDocument)
        {
            try
            {
                if (jsonPatchDocument == null)
                {
                    return BadRequest();
                }

                await _userSerivce.PartialUpdateAsync(id, jsonPatchDocument, User);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred during partial update for user with ID: {id}");
                throw;
            }
        }
    }
}
