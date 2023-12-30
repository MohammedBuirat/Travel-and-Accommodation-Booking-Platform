using Travel_and_Accommodation_API.DataAccess.Repositories.RepositoryImplementation;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Views;

namespace Travel_and_Accommodation_API.DataAccess.Repositories.IRepository
{
    public interface IReviewRepository : IRepository<Review>
    {
        public Task<HotelNumAndSumOfReviews> GetHotelNumAndSumOfReviewsAsync(Guid hotelId);
        public Task<HotelReviewSummary> GetHotelReviewSummaryAsync(Guid hotelId);
    }
}
