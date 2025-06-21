using System.Security.Claims;
using Travel_and_Accommodation_API.Dto.UserLastHotels;
using Travel_and_Accommodation_API.Models;

namespace Travel_and_Accommodation_API.Services.DataAccess.IServices
{
    public interface IUserLastHotelsService
    {
        public Task<UserLastHotels> AddAsync(UserLastHotels entity);
        public Task<IEnumerable<UserLastHotelsDto>> GetUsersLastHotelsAsync(string userId, ClaimsPrincipal user);
        public Task DeleteAsync(Guid hotelId, string userId, ClaimsPrincipal user);
    }
}
