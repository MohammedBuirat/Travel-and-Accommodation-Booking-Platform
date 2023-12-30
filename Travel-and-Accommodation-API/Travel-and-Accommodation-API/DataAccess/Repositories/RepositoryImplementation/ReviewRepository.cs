using Microsoft.EntityFrameworkCore;
using Travel_and_Accommodation_API.DataAccess.Repositories.IRepository;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Views;

namespace Travel_and_Accommodation_API.DataAccess.Repositories.RepositoryImplementation
{
    public class ReviewRepository : Repository<Review>, IReviewRepository
    {
        private readonly TravelAndAccommodationContext _dbContext;
        private readonly ILogger<ReviewRepository> _logger;

        public ReviewRepository(TravelAndAccommodationContext dbContext, ILogger<ReviewRepository> logger)
            : base(dbContext, logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
        public async Task<HotelNumAndSumOfReviews> GetHotelNumAndSumOfReviewsAsync(Guid hotelId)
        {
            try
            {
                var hotelReviews = await _dbContext.Reviews
                    .Where(r => r.HotelId == hotelId)
                    .GroupBy(r => r.HotelId)
                    .Select(g => new HotelNumAndSumOfReviews
                    {
                        NumOfReviews = g.Count(),
                        SumOfReviews = g.Sum(r => r.OverAllRating)
                    })
                    .FirstAsync();

                return hotelReviews;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving hotel reviews.");
                throw;
            }
        }

        public async Task<HotelReviewSummary> GetHotelReviewSummaryAsync(Guid hotelId)
        {
            try
            {
                var hotelReviews = await _dbContext.Reviews
                        .Where(r => r.HotelId == hotelId)
                        .GroupBy(r => r.HotelId)
                        .Select(g => new HotelReviewSummary
                        {
                            HotelId = g.Key,
                            NumOfReviews = g.Count(),
                            OverAllRatings = (decimal)g.Average(r => r.OverAllRating),
                            SecurityRating = (decimal)g.Average(r => r.SecurityRating ?? 0),
                            StaffRating = (decimal)g.Average(r => r.StaffRating ?? 0),
                            CleanlinessRating = (decimal)g.Average(r => r.CleanlinessRating ?? 0),
                            ValueForMoneyRating = (decimal)g.Average(r => r.ValueForMoneyRating ?? 0),
                            LocationRating = (decimal)g.Average(r => r.LocationRating ?? 0),
                            AtmosphereRating = (decimal)g.Average(r => r.AtmosphereRating ?? 0),
                            FacilitiesRating = (decimal)g.Average(r => r.FacilitiesRating ?? 0)
                        })
                        .FirstOrDefaultAsync();

                return hotelReviews;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while retrieving hotel {hotelId} review summary");
                throw;
            }
        }


    }

    public class HotelNumAndSumOfReviews
    {
        public int NumOfReviews { get; set; }
        public int SumOfReviews { get; set; }
    }
}
