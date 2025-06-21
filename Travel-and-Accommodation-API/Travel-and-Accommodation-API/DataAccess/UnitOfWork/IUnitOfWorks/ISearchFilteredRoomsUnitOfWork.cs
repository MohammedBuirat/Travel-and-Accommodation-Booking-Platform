using Travel_and_Accommodation_API.Helpers;
using Travel_and_Accommodation_API.Views;

namespace Travel_and_Accommodation_API.DataAccess.UnitOfWork.IUnitOfWorks
{
    public interface ISearchFilteredRoomsUnitOfWork
    {
        public Task<IEnumerable<RoomWithPrice>> GetSearchedFilteredRoomsAsync(RoomPriceFilterExpression expression);
    }
}
