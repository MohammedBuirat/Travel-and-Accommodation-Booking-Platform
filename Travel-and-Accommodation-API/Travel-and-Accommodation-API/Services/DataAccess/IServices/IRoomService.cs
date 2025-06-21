using Microsoft.AspNetCore.JsonPatch;
using Travel_and_Accommodation_API.Dto.Room;
using Travel_and_Accommodation_API.Helpers;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Views;
using static Travel_and_Accommodation_API.Models.Enums;

namespace Travel_and_Accommodation_API.Services.DataAccess.IServices
{
    public interface IRoomService
    {
        public Task<IEnumerable<RoomDto>> GetAllAsync(int? pageSize, int? pageNumber);
        public Task<RoomDto> GetByIdAsync(Guid id);
        public Task DeleteAsync(Guid id);
        public Task UpdateAsync(Guid id, RoomToAdd roomToUpdate);
        public Task<Room> AddAsync(RoomToAdd room);
        public Task PartialUpdateAsync(Guid id, JsonPatchDocument<Room> jsonPatchDocument);
        public Task<IEnumerable<RoomDto>> GetHotelRoomsAsync(Guid hotelId, int? pageSize, int? pageNumber);
        public Task<IEnumerable<RoomWithPriceDto>> GetSearchedFilteredRoomsAsync(Guid hotelId, int numberOfAdults, int numberOfChildren,
            DateTime checkInDate, DateTime checkOutDate, int? pageSize, int? pageNumber, List<string>? amenities,
            SortCriteria? sort, decimal? maxPrice, decimal? minPrice, RoomType? roomType, bool? descendingOrder);
    }
}
