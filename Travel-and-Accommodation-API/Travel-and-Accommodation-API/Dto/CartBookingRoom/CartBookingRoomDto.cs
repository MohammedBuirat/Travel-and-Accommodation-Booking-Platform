using System.ComponentModel.DataAnnotations;

namespace Travel_and_Accommodation_API.Dto.CartBookingRoom
{
    public class CartBookingRoomDto
    {
        public Guid Id { get; set; }
        [Required]
        public Guid RoomId { get; set; }
        [Required]
        public Guid CartBookingId { get; set; }
    }
}
