using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using System.Security.Claims;
using Travel_and_Accommodation_API.Dto.User;
using Travel_and_Accommodation_API.Exceptions_and_logs;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Services.DataAccess.IServices;
using Travel_and_Accommodation_API.Services.EmailService;
using Travel_and_Accommodation_API.Services.Validation;

namespace Travel_and_Accommodation_API.Services.DataAccess.Services
{
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly UserValidation _userValidation;
        public UserService(ILogger<UserService> logger,
            IMapper mapper,
            UserManager<User> userManager,
            UserValidation userValidation)
        {
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
            _userManager = userManager ??
                throw new ArgumentNullException(nameof(userManager));
            _userValidation = userValidation ??
                throw new ArgumentNullException(nameof(userValidation));
        }
        public async Task<UserDto> GetByIdAsync(string id)
        {
            try
            {
                User user = await GetUserAsync(id);

                UserDto userToReturn = _mapper.Map<UserDto>(user);
                _logger.LogInformation($"Successfully fetched user with ID: {id}");
                return userToReturn;
            }
            catch (ElementNotFoundException)
            {
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while fetching user with ID: {id}");
                throw;
            }
        }

        public async Task DeleteAsync(string id, ClaimsPrincipal userClaims)
        {
            try
            {
                AuthorizeUser(userClaims, id);
                var user = await GetUserAsync(id);

                user.Active = false;
                await _userManager.UpdateAsync(user);
                _logger.LogInformation($"Successfully deleted user with ID: {id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while deleting user with ID: {id}");
                throw;
            }
        }

        public async Task UpdateAsync(string id, User userToUpdate, ClaimsPrincipal userClaims)
        {
            try
            {
                AuthorizeUser(userClaims, id);
                var oldUser = await GetUserAsync(id);
                var newUser = oldUser.Clone();
                _mapper.Map(userToUpdate, newUser);
                await ValidateUserAsync(newUser);

                await _userManager.UpdateAsync(newUser);
                _logger.LogInformation($"Successfully updated user with ID: {id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while updating user with ID: {id}");
                throw;
            }
        }

        public async Task PartialUpdateAsync(string id, JsonPatchDocument<User> jsonPatchDocument, ClaimsPrincipal userClaims)
        {
            try
            {
                AuthorizeUser(userClaims, id);
                var oldUser = await GetUserAsync(id);
                var newUser = oldUser.Clone();
                jsonPatchDocument.ApplyTo(newUser);
                await ValidateUserAsync(newUser);

                await _userManager.UpdateAsync(newUser);
                _logger.LogInformation($"Successfully partially updated user with ID: {id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred during partial update for user with ID: {id}");
                throw;
            }
        }

        private async Task<User> GetUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                throw new ElementNotFoundException();
            }
            return user;
        }

        private async Task ValidateUserAsync(User entity)
        {
            var validationResults = await _userValidation.ValidateAsync(entity);
            if (!validationResults.IsValid)
            {
                throw new ValidationException(validationResults);
            }
        }

        private void AuthorizeUser(ClaimsPrincipal userClaims, string id)
        {
            var userId = userClaims.Claims.FirstOrDefault(c => c.Type == "Sub")?.Value;
            var userRole = userClaims.Claims.FirstOrDefault(c => c.Type == "Role")?.Value ?? "User";
            if (userId != id && userRole != "Admin")
            {
                throw new UnauthorizedAccessException();
            }
        }
    }
}