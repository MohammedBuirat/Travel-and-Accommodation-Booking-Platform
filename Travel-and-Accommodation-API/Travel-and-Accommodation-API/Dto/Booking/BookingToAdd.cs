using System.ComponentModel.DataAnnotations;

namespace Travel_and_Accommodation_API.Dto.Booking
{
    public class BookingToAdd
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public Guid HotelId { get; set; }
        [Required]
        public DateTime? CheckInDate { get; set; }
        [Required]
        public DateTime? CheckOutDate { get; set; }
        [Required]
        public decimal TotalPrice { get; set; }
        [Required]
        public string? SpecialRequests { get; set; }
        [Required]
        public bool? Paid { get; set; }
        [Required]
        public int? NumOfAdults { get; set; }
        [Required]
        public int? NumOfChildren { get; set; }
        [Required]
        public List<Guid> Rooms { get; set; }
    }
}
