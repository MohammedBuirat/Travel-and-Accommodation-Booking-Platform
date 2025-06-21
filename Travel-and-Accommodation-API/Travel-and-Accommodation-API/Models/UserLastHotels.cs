namespace Travel_and_Accommodation_API.Models
{
    public class UserLastHotels
    {
        public Guid Id { get; set; }
        public Guid HotelId { get; set; }
        public Hotel Hotel { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public DateTimeOffset DateOfView { get; set; }

        public override string ToString()
        {
            return $"{Id}, {HotelId}, {UserId}, {DateOfView}";
        }

        public UserLastHotels Clone()
        {
            return new UserLastHotels
            {
                Id = this.Id,
                HotelId = this.HotelId,
                Hotel = null,
                UserId = this.UserId,
                User = null,
                DateOfView = this.DateOfView
            };
        }
    }
}
