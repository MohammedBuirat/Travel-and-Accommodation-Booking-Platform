using System.Linq.Expressions;
using Travel_and_Accommodation_API.Models;

namespace Travel_and_Accommodation_API.DataAccess.Repositories.IRepository
{
    public interface IRoomDayRepository : IRepository<RoomDay>
    {
        public Task<RoomDay> GetLastRoomDay(Guid roomId);
    }
}
