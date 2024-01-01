using Microsoft.EntityFrameworkCore;
using Travel_and_Accommodation_API.Helpers;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Views;

namespace Travel_and_Accommodation_API.DataAccess.UnitOfWork.IUnitOfWorks
{
    public class SearchedFilteredHotelsUnitOfWork : ISearchedFilteredHotelsUnitOfWork
    {
        private readonly TravelAndAccommodationContext _dbContext;
        private readonly ILogger<SearchedFilteredHotelsUnitOfWork> _logger;

        public SearchedFilteredHotelsUnitOfWork(TravelAndAccommodationContext dbContext,
            ILogger<SearchedFilteredHotelsUnitOfWork> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
        }
        public async Task<IEnumerable<HotelWithPrice>> GetSearchedFilteredHotelsAsync(FilteredHotelRequest filter)
        {
            try
            {
                int rowBegin = (filter.Paging.PageNumber - 1) * filter.Paging.PageSize;
                int rowEnd = rowBegin + filter.Paging.PageSize;
                string sqlQuery = $"\n" +
                $"SELECT H.*, T.[TotalPrice], T.RoomId, T.Amenities\n" +
                $"FROM Hotels H\n" +
                $"INNER JOIN (\n" +
                $"   SELECT R.RoomId, Ro.HotelId, Ro.Amenities, SUM(R.Price) AS [TotalPrice],\n" +
                $"           ROW_NUMBER() OVER (PARTITION BY Ro.HotelId ORDER BY {filter.SortWindow} ) AS RowNum\n" +
                $"    FROM RoomDays R\n" +
                $"    INNER JOIN Rooms Ro ON R.RoomId = Ro.Id\n" +
                $"    WHERE {filter.RoomFilters}\n" +
                $"    GROUP BY R.RoomId, Ro.HotelId, Ro.Amenities\n" +
                $") AS T ON T.HotelId = H.Id\n" +
                $"WHERE {filter.HotelFilter}\n" +
                $"ORDER BY {filter.Sort} \n" +
                $"OFFSET {rowBegin} ROWS\n" +
                $"FETCH NEXT {rowEnd} ROWS ONLY;\n";
                var hotelsWithPrices = await _dbContext.HotelsWithPrice.FromSqlRaw(sqlQuery).ToListAsync();

                return hotelsWithPrices;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving Searched filter hotels");
                throw;
            }
        }
    }
}
