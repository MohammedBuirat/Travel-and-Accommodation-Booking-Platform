using Microsoft.AspNetCore.JsonPatch;
using System.Security.Claims;
using Travel_and_Accommodation_API.Dto.Booking;
using Travel_and_Accommodation_API.Models;

namespace Travel_and_Accommodation_API.Services.DataAccess.IServices
{
    public interface IBookingService
    {
        public Task<IEnumerable<BookingDto>> GetAllAsync(int? pageSize, int? pageNumber);
        public Task<BookingDto> GetByIdAsync(Guid id, ClaimsPrincipal userClaims);
        public Task DeleteAsync(Guid id, ClaimsPrincipal userClaims);
        public Task UpdateAsync(Guid id, BookingDto bookingToUpdate, ClaimsPrincipal userClaims);
        public Task<Booking> AddAsync(BookingToAdd bookingToAdd, ClaimsPrincipal userClaims);
        public Task PartialUpdateAsync(Guid id, JsonPatchDocument<Booking> jsonPatchDocument, ClaimsPrincipal userClaims);

        public Task<IEnumerable<BookingDto>> GetUserBookings(string userId,
           bool? previous, bool? upComing, int? pageNumber, int? pageSize, ClaimsPrincipal userClaims);
    }
}
