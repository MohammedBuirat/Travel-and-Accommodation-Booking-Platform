namespace Travel_and_Accommodation_API.Dto.CartBooking
{
    public class CartBookingDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string? SpecialRequests { get; set; }
        public DateTimeOffset? ModificationDate { get; set; }
        public DateTimeOffset CreationDate { get; set; }
        public int NumOfAdults { get; set; }
        public int NumOfChildren { get; set; }
    }
}
