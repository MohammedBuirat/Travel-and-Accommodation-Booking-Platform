namespace Travel_and_Accommodation_API.Models
{
    public class HotelImage
    {
        public Guid Id { get; set; }
        public string ImageString { get; set; }
        public Guid HotelId { get; set; }
        public Hotel Hotel { get; set; }

        public override string ToString()
        {
            return $"{Id}, {ImageString}, {HotelId}";
        }

        public HotelImage Clone()
        {
            return new HotelImage
            {
                Id = this.Id,
                ImageString = this.ImageString,
                HotelId = this.HotelId,
                Hotel = null
            };
        }
    }
}
