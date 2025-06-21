using System.ComponentModel.DataAnnotations;
using Travel_and_Accommodation_API.Services.ImageService;

namespace Travel_and_Accommodation_API.Dto.Hotel
{
    public class HotelToAdd
    {
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }
        [Required]
        public Guid CityId { get; set; }
        [Required]
        public string Owner { get; set; }
        [Required]
        public string CheckInTime { get; set; }
        [Required]
        public string CheckOutTime { get; set; }
        [Required]
        public String Address { get; set; }
        [Required]
        public double Latitude { get; set; }
        [Required]
        public double Longitude { get; set; }
        [Required]
        public decimal DistanceFromCityCenter { get; set; }
        public List<FileUpload> ImageFiles { get; set; }
    }
}
