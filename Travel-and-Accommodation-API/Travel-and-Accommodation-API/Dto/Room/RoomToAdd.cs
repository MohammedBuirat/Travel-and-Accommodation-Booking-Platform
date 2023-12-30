using System.ComponentModel.DataAnnotations;
using Travel_and_Accommodation_API.Services.ImageService;
using static Travel_and_Accommodation_API.Models.Enums;

namespace Travel_and_Accommodation_API.Dto.Room
{
    public class RoomToAdd
    {
        [Required]
        public int RoomNumber { get; set; }
        public string? Description { get; set; }

        [Required]
        public int AdultsCapacity { get; set; }
        [Required]
        public int ChildrenCapacity { get; set; }
        [Required]
        public RoomType Type { get; set; }
        public List<string> Amenities { get; set; }
        [Required]
        public Guid HotelId { get; set; }
        public FileUpload ImageFile { get; set; }
        [Required]
        public decimal BasePrice { get; set; }
    }
}
