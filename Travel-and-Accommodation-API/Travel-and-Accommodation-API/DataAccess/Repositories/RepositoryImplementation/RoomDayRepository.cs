using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Travel_and_Accommodation_API.DataAccess.Repositories.IRepository;
using Travel_and_Accommodation_API.Models;

namespace Travel_and_Accommodation_API.DataAccess.Repositories.RepositoryImplementation
{
    public class RoomDayRepository : Repository<RoomDay>, IRoomDayRepository
    {
        private readonly TravelAndAccommodationContext _dbContext;
        private readonly ILogger<RoomDayRepository> _logger;

        public RoomDayRepository(TravelAndAccommodationContext dbContext, ILogger<RoomDayRepository> logger)
            : base(dbContext, logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<RoomDay> GetLastRoomDay(Guid roomId)
        {
            try
            {
                var roomDay = await _dbContext.RoomDays
                    .Where(rd => rd.RoomId == roomId)
                    .OrderByDescending(rd => rd.Date)
                    .FirstOrDefaultAsync();

                return roomDay;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving the last room day.");
                throw;
            }
        }
    }
}
