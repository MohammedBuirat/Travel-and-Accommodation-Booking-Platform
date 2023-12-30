namespace Travel_and_Accommodation_API.Models
{
    public class Country
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Currency { get; set; }
        public string Language { get; set; }
        public string CountryCode { get; set; }
        public string TimeZone { get; set; }
        public string FlagImage { get; set; }

        public override string ToString()
        {
            return $"{Id}, {Name}, {Currency}, {Language}, {CountryCode}, {TimeZone}, {FlagImage}";
        }

        public Country Clone()
        {
            return new Country
            {
                Id = this.Id,
                Name = this.Name,
                Currency = this.Currency,
                Language = this.Language,
                CountryCode = this.CountryCode,
                TimeZone = this.TimeZone,
                FlagImage = this.FlagImage
            };
        }
    }
}
