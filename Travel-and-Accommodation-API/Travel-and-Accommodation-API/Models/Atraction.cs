namespace Travel_and_Accommodation_API.Models
{
    public class Attraction
    {
        public Guid Id {  get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public Guid CityId { get; set; }
        public City City { get; set; }
        public string Image { get; set; }

        public override string ToString()
        {
            return $"{Id}, {Name}, {Latitude}, {Longitude}, {CityId}, {Image}";
        }

        public Attraction Clone()
        {
            return new Attraction
            {
                Id = this.Id,
                Name = this.Name,
                Latitude = this.Latitude,
                Longitude = this.Longitude,
                CityId = this.CityId,
                City = null,
                Image = this.Image
            };
        }
    }
}
