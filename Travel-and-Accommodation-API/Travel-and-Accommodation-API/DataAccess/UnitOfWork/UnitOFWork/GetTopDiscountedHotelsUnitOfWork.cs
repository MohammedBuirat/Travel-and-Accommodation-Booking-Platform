using Microsoft.EntityFrameworkCore;
using Travel_and_Accommodation_API.Helpers;
using Travel_and_Accommodation_API.Views;

namespace Travel_and_Accommodation_API.DataAccess.UnitOfWork.IUnitOfWorks
{
    public class GetTopDiscountedHotelsUnitOfWork : IGetTopDiscountedHotelsUnitOfWork
    {
        private readonly TravelAndAccommodationContext _dbContext;
        private readonly ILogger<GetTopDiscountedHotelsUnitOfWork> _logger;
        public GetTopDiscountedHotelsUnitOfWork(TravelAndAccommodationContext dbContext
            , ILogger<GetTopDiscountedHotelsUnitOfWork> logger)
        {
            _dbContext = dbContext ??
                throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
        }
        public async Task<IEnumerable<HotelRoomWithDiscount>> GetTopDiscountedHotelsAsync(Paging paging)
        {
            try
            {
                var topDiscountedHotels = await (from h in _dbContext.Hotels
                                                 join r in _dbContext.Rooms on h.Id equals r.HotelId
                                                 join rd in _dbContext.RoomDays on r.Id equals rd.RoomId
                                                 where (1 - (rd.Price / r.BasePrice)) != 0 &&
                                                 rd.Available == true
                                                 orderby (1 - (rd.Price / r.BasePrice)) descending
                                                 select new HotelRoomWithDiscount
                                                 {
                                                     Id = h.Id,
                                                     Name = h.Name,
                                                     Description = h.Description,
                                                     CityId = h.CityId,
                                                     Owner = h.Owner,
                                                     CheckInTime = h.CheckInTime,
                                                     CheckOutTime = h.CheckOutTime,
                                                     NumOfRatings = h.NumOfRatings,
                                                     SumOfRatings = h.SumOfRatings,
                                                     Address = h.Address,
                                                     Latitude = h.Latitude,
                                                     Longitude = h.Longitude,
                                                     CreationDate = h.CreationDate,
                                                     ModificationDate = h.ModificationDate,
                                                     CityName = h.City.Name,
                                                     RoomId = r.Id,
                                                     Date = rd.Date,
                                                     BasePrice = r.BasePrice,
                                                     DiscountedPrice = rd.Price,
                                                     DiscountPercentage = (1 - (rd.Price / r.BasePrice)) * 100
                                                 }).Skip((paging.PageNumber - 1) * paging.PageSize)
                                                  .Take(paging.PageSize).ToListAsync();

                return topDiscountedHotels;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error while retrieving discounted hotels");
                throw;
            }
        }


    }
}
