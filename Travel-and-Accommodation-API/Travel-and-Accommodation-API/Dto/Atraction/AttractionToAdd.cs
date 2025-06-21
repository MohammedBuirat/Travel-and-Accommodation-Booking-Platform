using System.ComponentModel.DataAnnotations;
using Travel_and_Accommodation_API.Services.ImageService;

namespace Travel_and_Accommodation_API.Dto.Attraction
{
    public class AttractionToAdd
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public double Latitude { get; set; }
        [Required]
        public double Longitude { get; set; }
        [Required]
        public Guid CityId { get; set; }
        public FileUpload ImageFile { get; set; }
    }
}
