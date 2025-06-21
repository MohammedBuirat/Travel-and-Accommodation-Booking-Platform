using Travel_and_Accommodation_API.Services.ImageService;

namespace Travel_and_Accommodation_API.Dto.HotelImage
{
    public class HotelImageToAdd
    {
        public FileUpload ImageFile { get; set; }
        public Guid HotelId { get; set; }
    }
}
