using Microsoft.EntityFrameworkCore;
using Travel_and_Accommodation_API.DataAccess.Repositories.RepositoryImplementation;
using Travel_and_Accommodation_API.DataAccess.UnitOfWork.IUnitOfWorks;
using Travel_and_Accommodation_API.Helpers;
using Travel_and_Accommodation_API.Models;

namespace Travel_and_Accommodation_API.DataAccess.UnitOfWork.UnitOFWork
{
    public class MostVisitedCitiesUnitOfWork : IMostVisitedCitiesUnitOfWork
    {
        private readonly TravelAndAccommodationContext _dbContext;
        private readonly ILogger<MostVisitedCitiesUnitOfWork> _logger;
        public MostVisitedCitiesUnitOfWork(TravelAndAccommodationContext dbContext
            , ILogger<MostVisitedCitiesUnitOfWork> logger)
        {
            _dbContext = dbContext ??
                throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
        }

        public void SaveChanges()
        {
            _dbContext.SaveChanges();
        }

        public async Task<IEnumerable<City>> MostVisitedCitiesAsync(Paging paging)
        {
            try
            {
                var mostVisitedCities = await _dbContext.Cities
                    .Where(c => _dbContext.Bookings
                    .Join(_dbContext.Hotels, booking => booking.HotelId, hotel => hotel.Id, (booking, hotel) => new { booking, hotel })
                    .Where(joinResult => joinResult.hotel.CityId == c.Id)
                    .GroupBy(joinResult => joinResult.hotel.CityId)
                    .OrderByDescending(group => group.Count())
                    .Select(group => group.Key)
                    .Skip((paging.PageNumber - 1) * paging.PageSize)
                    .Take(paging.PageSize)
                    .Contains(c.Id))
                    .ToListAsync();

                return mostVisitedCities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving most visited cities");
                throw;
            }
        }
    }
}
