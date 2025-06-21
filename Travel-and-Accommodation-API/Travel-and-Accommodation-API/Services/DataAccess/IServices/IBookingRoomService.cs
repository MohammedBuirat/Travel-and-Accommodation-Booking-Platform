using System.Security.Claims;
using Travel_and_Accommodation_API.Dto.BookingRoom;
using Travel_and_Accommodation_API.Models;
namespace Travel_and_Accommodation_API.Services.DataAccess.IServices
{
    public interface IBookingRoomService
    {
        public Task<BookingRoom> AddAsync(BookingRoomDto entity, ClaimsPrincipal userClaims);
        public Task DeleteAsync(Guid bookingId, Guid roomId, ClaimsPrincipal userClaims);
        public Task<IEnumerable<BookingRoomDto>> GetBookingRoomsByBookingIdAsync(Guid bookingId, int? pageSize, int? pageNumber,
                    ClaimsPrincipal userClaims);
        public Task<IEnumerable<BookingRoomDto>> GetBookingRoomsByRoomIdIdAsync(Guid roomId, int? pageSize, int? pageNumber,
                    ClaimsPrincipal userClaims);
    }
}
