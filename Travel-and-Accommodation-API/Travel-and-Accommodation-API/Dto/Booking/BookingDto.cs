namespace Travel_and_Accommodation_API.Dto.Booking
{
    public class BookingDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string? SpecialRequests { get; set; }
        public DateTimeOffset BookingDate { get; set; }
        public bool Paid { get; set; }
        public int ConfirmationNumber { get; set; }
        public int NumOfAdults { get; set; }
        public int NumOfChildren { get; set; }
    }
}
