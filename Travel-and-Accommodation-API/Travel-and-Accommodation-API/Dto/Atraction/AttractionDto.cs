namespace Travel_and_Accommodation_API.Dto.Attraction
{
    public class AttractionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public Guid CityId { get; set; }
        public string Image { get; set; }
    }
}
