namespace Travel_and_Accommodation_API.Dto.UserLastHotels
{
    public class UserLastHotelsDto
    {
        public Guid Id { get; set; }
        public Guid HotelId { get; set; }
        public Guid UserId { get; set; }
        public DateTimeOffset DateOfView { get; set; }
    }
}
