using static Travel_and_Accommodation_API.Models.Enums;

namespace Travel_and_Accommodation_API.Helpers
{
    public class FilteredHotelRequest
    {
        public string RoomFilters { get; set; }
        public string HotelFilter { get; set; }
        public Paging Paging { get; set; }
        public string SortWindow { get; set; }
        public string Sort { get; set; }

        public FilteredHotelRequest(Guid cityId, int numberOfAdults, int numberOfChildren,
            DateTime checkInDate, DateTime checkOutDate, int? pageSize, int? pageNumber, List<string>? amenities,
            SortCriteria? sort, decimal? maxPrice, decimal? minPrice, int? minRatings, RoomType? roomType,
            bool? descendingOrder, decimal? distanceFromCityCenter)
        {
            Paging = new Paging(pageSize, pageNumber);
            RoomFilters = $"R.Date >= '{checkInDate.Date}'\n" +
                $"AND R.Date < '{checkOutDate.Date}'\n" +
                $"AND R.Available = 1 \n";
            if(numberOfChildren == 0)
            {
                RoomFilters += $"And Ro.AdultsCapacity >= {numberOfAdults} \n";
            }
            else
            {
                RoomFilters += $"And Ro.AdultsCapacity >= {numberOfAdults} \n" +
                    $"And (Ro.ChildrenCapacity + Ro.AdultsCapacity) >= {numberOfAdults + numberOfChildren}\n";
            }
            if(roomType != null)
            {
                RoomFilters += $"And Ro.Type = {(int)roomType}\n";
            }
            var aminitiesConverted = amenities.ConvertStringsToAmenities();
            if(aminitiesConverted != 0)
            {
                RoomFilters += $"And (Ro.Amenities &{aminitiesConverted}) = {aminitiesConverted}\n";
            }

            HotelFilter = $"T.RowNum = 1\n" +
                $"And H.CityId = '{cityId}'\n";
            if(minPrice != null && minPrice != 0)
            {
                HotelFilter += $"And T.Total-Price >= {minPrice}\n";
            }
            if(maxPrice != null)
            {
                HotelFilter += $"And T.Total-Price <= {maxPrice}\n";

            }
            if (minRatings != null)
            {
                HotelFilter += $"And (H.SumOfRatings/H.NumOfRatings) >= {minRatings}\n";
            }
            if (distanceFromCityCenter != null)
            {
                HotelFilter += $"And H.DistanceFromCityCenter <= {distanceFromCityCenter}\n";
            }
            SortWindow = GetSortStringWindow(sort);
            Sort =  GetSortString(sort);
            if (descendingOrder != null && descendingOrder == true)
            {
                if (Sort != "")
                {
                    Sort += " DESC";
                }
                SortWindow += " DESC";
            }
        }
        public static string GetSortStringWindow(SortCriteria? criteria)
        {
            if(criteria == null)
            {
                return "(SELECT NULL)";
            }
            switch (criteria)
            {
                case SortCriteria.Price:
                    return "SUM(R.Price)";
                case SortCriteria.Rating:
                    return "(H.SumOfRatings / H.NumOfRatings)";
                case SortCriteria.DistanceFromCity:
                    return "H.DistanceFromCityCenter";
                default:
                    return "(SELECT NULL)";
            }
        }

        public static string GetSortString(SortCriteria? criteria)
        {
            if (criteria == null)
            {
                return "";
            }
            switch (criteria)
            {
                case SortCriteria.Price:
                    return "[TotalPrice]";
                case SortCriteria.Rating:
                    return "(H.SumOfRatings / H.NumOfRatings)";
                case SortCriteria.DistanceFromCity:
                    return "H.DistanceFromCityCenter";
                default:
                    return "";
            }
        }
    }
}
