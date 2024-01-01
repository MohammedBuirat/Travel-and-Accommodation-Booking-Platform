namespace Travel_and_Accommodation_API.Dto.City
{
    public class CityDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string PostOffice { get; set; }
        public Guid CountryId { get; set; }
        public DateTimeOffset CreationDate { get; set; }
        public DateTimeOffset? ModificationDate { get; set; }
    }
}
