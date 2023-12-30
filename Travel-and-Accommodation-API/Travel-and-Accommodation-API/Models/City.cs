namespace Travel_and_Accommodation_API.Models
{
    public class City
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string PostOffice { get; set; }
        public string Image { get; set; }
        public Guid CountryId { get; set; }
        public Country Country { get; set; }
        public string CountryName { get; set; }
        public DateTimeOffset CreationDate { get; set; }
        public DateTimeOffset? ModificationDate { get; set; }
        public List<Hotel> Hotels { get; set; }

        public override string ToString()
        {
            return $"{Id}, {Name}, {PostOffice}, {Image}, {CountryId}, {CountryName}, " +
                   $"{CreationDate}, {ModificationDate}";
        }

        public City Clone()
        {
            return new City
            {
                Id = this.Id,
                Name = this.Name,
                PostOffice = this.PostOffice,
                Image = this.Image,
                CountryId = this.CountryId,
                Country = null,
                CountryName = this.CountryName,
                CreationDate = this.CreationDate,
                ModificationDate = this.ModificationDate,
                Hotels = null
            };
        }
    }
}
