using System.Linq.Expressions;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Views;

namespace Travel_and_Accommodation_API.Helpers
{
    public class RoomPriceFilterExpression
    {
        public Expression<Func<RoomDay, bool>> RoomDayFilter { get; set; }
        public Expression<Func<Room, bool>> RoomFilter { get; set; }
        public Expression<Func<RoomWithPrice, decimal>>? SortExpression { get; set; }
        public Paging Paging { get; set; }
        public bool? Desc { get; set; } = false;

        public RoomPriceFilterExpression(
            Expression<Func<RoomDay, bool>> RoomDayFilter,
            Expression<Func<Room, bool>> RoomFilter,
            Paging Paging,
            SortCriteria? sortCriteria,
            bool? desc = false)
        {
            this.Paging = Paging;
            this.RoomDayFilter = RoomDayFilter;
            this.RoomFilter = RoomFilter;
            Desc = desc;

            switch (sortCriteria)
            {
                case SortCriteria.Price:
                    SortExpression = rwp => rwp.TotalPrice;
                        break;

                default:
                    SortExpression = null;
                    break;
            }
        }
    }
}
