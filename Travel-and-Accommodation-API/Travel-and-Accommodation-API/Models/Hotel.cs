namespace Travel_and_Accommodation_API.Models
{
    public class Hotel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public City City { get; set; }
        public Guid CityId { get; set; }
        public string Owner { get; set; }
        public string CheckInTime { get; set; }
        public string CheckOutTime { get; set; }
        public int NumOfRatings { get; set; }
        public int SumOfRatings { get; set; }
        public String Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public List<HotelImage> Images { get; set; }
        public DateTimeOffset CreationDate { get; set; }
        public DateTimeOffset? ModificationDate { get; set; }
        public List<Review> Reviews { get; set; }
        public List<Room> Rooms { get; set; }
        public decimal DistanceFromCityCenter { get; set; }

        public override string ToString()
        {
            return $"{Id}, {Name}, {Description}, {CityId}, {Owner}, {CheckInTime}, " +
                   $"{CheckOutTime}, {NumOfRatings}, {SumOfRatings}, {Address}, " +
                   $"{Latitude}, {Longitude}, {CreationDate}, {ModificationDate}, " +
                   $"{DistanceFromCityCenter}";
        }

        public Hotel Clone()
        {
            return new Hotel
            {
                Id = this.Id,
                Name = this.Name,
                Description = this.Description,
                CityId = this.CityId,
                City = null,
                Owner = this.Owner,
                CheckInTime = this.CheckInTime,
                CheckOutTime = this.CheckOutTime,
                NumOfRatings = this.NumOfRatings,
                SumOfRatings = this.SumOfRatings,
                Address = this.Address,
                Latitude = this.Latitude,
                Longitude = this.Longitude,
                Images = null,
                CreationDate = this.CreationDate,
                ModificationDate = this.ModificationDate,
                Reviews = null,
                Rooms = null,
                DistanceFromCityCenter = this.DistanceFromCityCenter
            };
        }
    }
}
