using System.ComponentModel.DataAnnotations;

namespace Travel_and_Accommodation_API.Dto.CartBooking
{
    public class CartBookingToAdd
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public DateTime? CheckInDate { get; set; }
        [Required]
        public DateTime? CheckOutDate { get; set; }
        [Required]
        public string? SpecialRequests { get; set; }
        [Required]
        public int? NumOfAdults { get; set; }
        [Required]
        public int? NumOfChildren { get; set; }
        [Required]
        public List<Guid> Rooms { get; set; }
    }
}
