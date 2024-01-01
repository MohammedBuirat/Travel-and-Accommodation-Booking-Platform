namespace Travel_and_Accommodation_API.Dto.Country
{
    public class CountryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Currency { get; set; }
        public string Language { get; set; }
        public string CountryCode { get; set; }
        public string TimeZone { get; set; }
        public string FlagImage { get; set; }
    }
}
