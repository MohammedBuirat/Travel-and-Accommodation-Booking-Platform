using static Travel_and_Accommodation_API.Models.Enums;

namespace Travel_and_Accommodation_API.Dto.Room
{
    public class RoomDto
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
        public Guid HotelId { get; set; }
        public decimal BasePrice { get; set; }
    }
}
