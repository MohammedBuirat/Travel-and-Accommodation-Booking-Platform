using static Travel_and_Accommodation_API.Models.Enums;
using Travel_and_Accommodation_API.Models;

namespace Travel_and_Accommodation_API.Views
{
    public class RoomWithPrice
    {
        public Guid Id { get; set; }
        public int RoomNumber { get; set; }
        public string? Description { get; set; }
        public int AdultsCapacity { get; set; }
        public int ChildrenCapacity { get; set; }
        public RoomType Type { get; set; }
        public DateTimeOffset CreationDate { get; set; }
        public DateTimeOffset? ModificationDate { get; set; }
        public Amenities Amenities { get; set; }
        public Guid HotelId { get; set; }
        public List<RoomDay> RoomDays { get; set; }
        public string Image { get; set; }
        public decimal BasePrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class RoomWithPriceDto
    {
        public Guid Id { get; set; }
        public int RoomNumber { get; set; }
        public string? Description { get; set; }
        public int AdultsCapacity { get; set; }
        public int ChildrenCapacity { get; set; }
        public RoomType Type { get; set; }
        public DateTimeOffset CreationDate { get; set; }
        public DateTimeOffset? ModificationDate { get; set; }
        public List<string> Amenities { get; set; }
        public int HotelId { get; set; }
        public List<RoomDay> RoomDays { get; set; }
        public string Image { get; set; }
        public decimal BasePrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
