using Microsoft.AspNetCore.JsonPatch;
using System.Security.Claims;
using Travel_and_Accommodation_API.Dto.Review;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Views;

namespace Travel_and_Accommodation_API.Services.DataAccess.IServices
{
    public interface IReviewService
    {
        public Task<IEnumerable<ReviewDto>> GetAllAsync(int? pageSize, int? pageNumber);
        public Task<ReviewDto> GetByIdAsync(Guid id);
        public Task DeleteAsync(Guid id, ClaimsPrincipal user);
        public Task UpdateAsync(Guid id, ReviewDto reviewToUpdate, ClaimsPrincipal user);
        public Task<Review> AddAsync(ReviewToAdd review, ClaimsPrincipal user);
        public Task PartialUpdateAsync(Guid id, JsonPatchDocument<Review> jsonPatchDocument, ClaimsPrincipal user);
        public Task<IEnumerable<ReviewDto>> GetHotelReviewsAsync(Guid hotelId, int? pageSize, int? pageNumber);
        public Task<IEnumerable<ReviewDto>> GetUserReviewsAsync(string userId, int? pageSize, int? pageNumber);
        public Task<HotelReviewSummary> GetHotelReviewsSummaryAsync(Guid hotelId);
    }
}
