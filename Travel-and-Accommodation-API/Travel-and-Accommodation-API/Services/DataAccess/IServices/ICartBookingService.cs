using Microsoft.AspNetCore.JsonPatch;
using System.Security.Claims;
using Travel_and_Accommodation_API.Dto.CartBooking;
using Travel_and_Accommodation_API.Models;

namespace Travel_and_Accommodation_API.Services.DataAccess.IServices
{
    public interface ICartBookingService
    {
        public Task<IEnumerable<CartBookingDto>> GetAllAsync(int? pageSize, int? pageNumber);
        public Task<CartBookingDto> GetByIdAsync(Guid id, ClaimsPrincipal userClaims);
        public Task<CartBooking> AddAsync(CartBookingToAdd bookingToAdd, ClaimsPrincipal userClaims);
        public Task UpdateAsync(Guid id, CartBookingDto bookingToUpdate, ClaimsPrincipal userClaims);
        public Task DeleteAsync(Guid id, ClaimsPrincipal userClaims);
        public Task PartialUpdateAsync(Guid id, JsonPatchDocument<CartBooking> jsonPatchDocument, ClaimsPrincipal userClaims);
        public Task<IEnumerable<CartBookingDto>> GetUserBookingsAsync(string userId, int? pageNumber,
            int? pageSize, ClaimsPrincipal userClaims);

    }
}
