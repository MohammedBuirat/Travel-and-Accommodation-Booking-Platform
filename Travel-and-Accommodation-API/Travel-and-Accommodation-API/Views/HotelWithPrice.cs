using static Travel_and_Accommodation_API.Models.Enums;

namespace Travel_and_Accommodation_API.Views
{
    public class HotelWithPrice
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public Guid CityId { get; set; }
        public string Owner { get; set; }
        public Amenities Amenities { get; set; }
        public string CheckInTime { get; set; }
        public string CheckOutTime { get; set; }
        public int NumOfRatings { get; set; }
        public int SumOfRatings { get; set; }
        public String Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTimeOffset CreationDate { get; set; }
        public DateTimeOffset? ModificationDate { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal DistanceFromCityCenter { get; set; }
    }

    public class HotelWithPriceDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public Guid CityId { get; set; }
        public string Owner { get; set; }
        public List<string> Amenities { get; set; }
        public string CheckInTime { get; set; }
        public string CheckOutTime { get; set; }
        public int NumOfRatings { get; set; }
        public int SumOfRatings { get; set; }
        public String Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTimeOffset CreationDate { get; set; }
        public DateTimeOffset? ModificationDate { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal DistanceFromCityCenter { get; set; }
    }
}
