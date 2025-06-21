using Microsoft.AspNetCore.JsonPatch;
using System.Security.Claims;
using Travel_and_Accommodation_API.Dto.User;
using Travel_and_Accommodation_API.Models;

namespace Travel_and_Accommodation_API.Services.DataAccess.IServices
{
    public interface IUserService
    {
        Task<UserDto> GetByIdAsync(string id);
        Task DeleteAsync(string id, ClaimsPrincipal userClaims);
        Task UpdateAsync(string id, User userToUpdate, ClaimsPrincipal userClaims);
        Task PartialUpdateAsync(string id, JsonPatchDocument<User> jsonPatchDocument, ClaimsPrincipal userClaims);
    }
}
