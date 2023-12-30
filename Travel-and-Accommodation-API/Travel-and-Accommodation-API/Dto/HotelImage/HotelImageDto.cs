namespace Travel_and_Accommodation_API.Dto.HotelImage
{
    public class HotelImageDto
    {
        public Guid Id { get; set; }
        public string ImageString { get; set; }
        public Guid HotelId { get; set; }
    }
}
