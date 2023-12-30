using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Travel_and_Accommodation_API.Dto.Hotel;
using Travel_and_Accommodation_API.Helpers;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Views;
using static Travel_and_Accommodation_API.Models.Enums;

namespace Travel_and_Accommodation_API.Services.DataAccess.IServices
{
    public interface IHotelService
    {
        public Task<IEnumerable<HotelDto>> GetAllAsync(int? pageSize, int? pageNumber);
        public Task<ActionResult<HotelDto>> GetByIdAsync(Guid id, ClaimsPrincipal userClaims);
        public Task DeleteAsync(Guid id);
        public Task UpdateAsync(Guid id, HotelToAdd hotelToUpdate);
        public Task<Hotel> AddAsync(HotelToAdd hotel);
        public Task PartialUpdateAsync(Guid id, JsonPatchDocument<Hotel> jsonPatchDocument);
        public Task<IEnumerable<HotelDto>> GetCityHotelsAsync(Guid cityId, int? pageSize, int? pageNum);
        public Task<ActionResult<IEnumerable<HotelDto>>> AdminSearchHotelsAsync(string? name, int? pageSize, int? pageNumber,
            Guid? cityId, DateTimeOffset? creationDate, decimal? ratings);
        public Task<IEnumerable<HotelRoomWithDiscount>> GetDiscountedHotelRoomsAsync(int? pageSize, int? pageNumber);
        public Task<IEnumerable<HotelWithPriceDto>> GetSearchedFilteredHotelsAsync(Guid cityId, int numberOfAdults, int numberOfChildren,
            DateTime checkInDate, DateTime checkOutDate, int? pageSize, int? pageNumber, List<string>? amenities,
            SortCriteria? sort, decimal? maxPrice, decimal? minPrice, int? minRatings, RoomType? roomType,
            bool? descendingOrder, decimal? distanceFromCityCenter);
        public Task EditHotelTotalReviewsAsync(Guid hotelId);
    }
}
