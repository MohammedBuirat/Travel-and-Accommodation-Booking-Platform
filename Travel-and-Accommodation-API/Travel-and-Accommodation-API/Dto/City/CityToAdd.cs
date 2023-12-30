using System.ComponentModel.DataAnnotations;
using Travel_and_Accommodation_API.Services.ImageService;

namespace Travel_and_Accommodation_API.Dto.City
{
    public class CityToAdd
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string PostOffice { get; set; }
        [Required]
        public FileUpload ImageFile { get; set; }
        [Required]
        public Guid CountryId { get; set; }
    }
}
