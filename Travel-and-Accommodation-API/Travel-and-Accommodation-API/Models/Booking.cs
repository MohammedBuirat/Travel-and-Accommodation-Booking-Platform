namespace Travel_and_Accommodation_API.Models
{
    public class Booking
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public Guid HotelId { get; set; }
        public Hotel Hotel { get; set; }
        public List<BookingRoom> BookingRooms { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string? SpecialRequests { get; set; }
        public DateTimeOffset BookingDate { get; set; }
        public bool Paid { get; set; }
        public int ConfirmationNumber { get; set; }
        public int NumOfAdults { get; set; }
        public int NumOfChildren { get; set; }

        public override string ToString()
        {
            return $"{Id}, {UserId}, {HotelId}, {CheckInDate}, {CheckOutDate}, " +
                   $"{TotalPrice}, {SpecialRequests}, {BookingDate}, " +
                   $"{Paid}, {ConfirmationNumber}, {NumOfAdults}, {NumOfChildren}";
        }

        public Booking Clone()
        {
            return new Booking
            {
                Id = this.Id,
                UserId = this.UserId,
                User = null,
                HotelId = this.HotelId,
                Hotel = null,
                BookingRooms = null,
                CheckInDate = this.CheckInDate,
                CheckOutDate = this.CheckOutDate,
                TotalPrice = this.TotalPrice,
                SpecialRequests = this.SpecialRequests,
                BookingDate = this.BookingDate,
                Paid = this.Paid,
                ConfirmationNumber = this.ConfirmationNumber,
                NumOfAdults = this.NumOfAdults,
                NumOfChildren = this.NumOfChildren
            };
        }
    }
}
