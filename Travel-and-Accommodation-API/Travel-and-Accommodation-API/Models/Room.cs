using static Travel_and_Accommodation_API.Models.Enums;

namespace Travel_and_Accommodation_API.Models
{
    public class Room
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
        public List<BookingRoom> BookingRooms { get; set; }
        public Guid HotelId { get; set; }
        public List<RoomDay> RoomDays { get; set; }
        public Hotel Hotel { get; set; }
        public string Image { get; set; }
        public decimal BasePrice { get; set; }

        public override string ToString()
        {
            return $"{Id}, {RoomNumber}, {Description}, {AdultsCapacity}, {ChildrenCapacity}, " +
                   $"{Type}, {CreationDate}, {ModificationDate}, {Amenities}, {HotelId}, " +
                   $"{Image}, {BasePrice}";
        }

        public Room Clone()
        {
            return new Room
            {
                Id = this.Id,
                RoomNumber = this.RoomNumber,
                Description = this.Description,
                AdultsCapacity = this.AdultsCapacity,
                ChildrenCapacity = this.ChildrenCapacity,
                Type = this.Type,
                CreationDate = this.CreationDate,
                ModificationDate = this.ModificationDate,
                Amenities = this.Amenities,
                BookingRooms = null,
                HotelId = this.HotelId,
                RoomDays = null,
                Hotel = null,
                Image = this.Image,
                BasePrice = this.BasePrice
            };
        }
    }
}
