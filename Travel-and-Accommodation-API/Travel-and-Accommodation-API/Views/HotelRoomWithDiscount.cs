namespace Travel_and_Accommodation_API.Views
{
    public class HotelRoomWithDiscount
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public Guid CityId { get; set; }
        public string Owner { get; set; }
        public string CheckInTime { get; set; }
        public string CheckOutTime { get; set; }
        public int NumOfRatings { get; set; }
        public decimal SumOfRatings { get; set; }
        public String Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTimeOffset CreationDate { get; set; }
        public DateTimeOffset? ModificationDate { get; set; }
        public string CityName { get; set; }
        public Guid RoomId { get; set; }
        public DateTime Date { get; set; }
        public decimal BasePrice { get; set; }
        public decimal DiscountedPrice { get; set; }
        public decimal DiscountPercentage { get; set; }
    }
}
