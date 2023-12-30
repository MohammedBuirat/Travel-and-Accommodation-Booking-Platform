using System.Security.Claims;
using Travel_and_Accommodation_API.Dto.CartBookingRoom;
using Travel_and_Accommodation_API.Models;

namespace Travel_and_Accommodation_API.Services.DataAccess.IServices
{
    public interface ICartBookingRoomService
    {
        public Task<CartBookingRoom> AddAsync(CartBookingRoomDto entity, ClaimsPrincipal userClaims);
        public Task DeleteAsync(Guid bookingId, Guid roomId, ClaimsPrincipal userClaims);
        public Task<IEnumerable<CartBookingRoomDto>> GetBookingRoomsByBookingIdAsync(Guid bookingId, int? pageSize, int? pageNumber,
            ClaimsPrincipal userClaims);
    }
}
