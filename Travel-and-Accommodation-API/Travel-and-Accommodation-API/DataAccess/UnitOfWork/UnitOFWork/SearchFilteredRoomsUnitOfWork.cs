using Microsoft.EntityFrameworkCore;
using Travel_and_Accommodation_API.Helpers;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Views;

namespace Travel_and_Accommodation_API.DataAccess.UnitOfWork.IUnitOfWorks
{
    public class SearchFilteredRoomsUnitOfWork : ISearchFilteredRoomsUnitOfWork
    {
        private readonly TravelAndAccommodationContext _dbContext;
        private readonly ILogger<SearchedFilteredHotelsUnitOfWork> _logger;

        public SearchFilteredRoomsUnitOfWork(TravelAndAccommodationContext dbContext
            , ILogger<SearchedFilteredHotelsUnitOfWork> logger)
        {
            _dbContext = dbContext ??
                throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
        }
        public async Task<IEnumerable<RoomWithPrice>> GetSearchedFilteredRoomsAsync(RoomPriceFilterExpression expression)
        {
            try
            {
                if (expression == null)
                {
                    throw new ArgumentNullException(nameof(expression));
                }

                IQueryable<RoomDay> roomDays = _dbContext.Set<RoomDay>();
                IQueryable<Room> rooms = _dbContext.Set<Room>();

                if (expression.RoomDayFilter != null)
                {
                    roomDays = roomDays.Where(expression.RoomDayFilter);
                }

                if (expression.RoomFilter != null)
                {
                    rooms = rooms.Where(expression.RoomFilter);
                }

                var roomDaysTotalPriceQuery =
                    from rd in roomDays
                    group rd by rd.RoomId into groupResult
                    select new
                    {
                        RoomId = groupResult.Key,
                        TotalPrice = groupResult.Sum(rd => rd.Price)
                    };

                var joinedQuery =
                    from rdt in roomDaysTotalPriceQuery
                    join r in rooms on rdt.RoomId equals r.Id
                    select new RoomWithPrice
                    {
                        Id = r.Id,
                        RoomNumber = r.RoomNumber,
                        Description = r.Description,
                        AdultsCapacity = r.AdultsCapacity,
                        ChildrenCapacity = r.ChildrenCapacity,
                        Type = r.Type,
                        CreationDate = r.CreationDate,
                        ModificationDate = r.ModificationDate,
                        Amenities = r.Amenities,
                        HotelId = r.HotelId,
                        Image = r.Image,
                        BasePrice = r.BasePrice,
                        TotalPrice = rdt.TotalPrice
                    };


                if (expression.SortExpression != null)
                {

                    joinedQuery = (expression.Desc == true) ?
                        joinedQuery.OrderByDescending(expression.SortExpression) :
                        joinedQuery.OrderBy(expression.SortExpression);

                }

                var paginatedQuery = joinedQuery.Skip((expression.Paging.PageNumber - 1) * expression.Paging.PageSize)
                                         .Take(expression.Paging.PageSize);

                var result = await paginatedQuery.ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving searched hotel rooms");
                throw;
            }
        }

    }
}
