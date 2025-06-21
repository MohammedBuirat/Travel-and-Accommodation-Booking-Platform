using System.ComponentModel.DataAnnotations;
using Travel_and_Accommodation_API.Services.ImageService;

namespace Travel_and_Accommodation_API.Dto.Country
{
    public class CountryToAdd
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Currency { get; set; }
        [Required]
        public string Language { get; set; }
        [Required]
        public string CountryCode { get; set; }
        [Required]
        public string TimeZone { get; set; }
        [Required]
        public FileUpload ImageFile { get; set; }
    }
}
