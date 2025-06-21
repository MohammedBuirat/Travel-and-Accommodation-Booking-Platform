namespace Travel_and_Accommodation_API.Models
{
    public class CartBooking
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public List<CartBookingRoom> BookingRooms { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string? SpecialRequests { get; set; }
        public DateTimeOffset CreationDate { get; set; }
        public DateTimeOffset? ModificationDate { get; set; }
        public int NumOfAdults { get; set; }
        public int NumOfChildren { get; set; }

        public string PrintValues()
        {
            return $"{Id}, {UserId}, {CheckInDate}, {CheckOutDate}, " +
                   $"{SpecialRequests}, " +
                   $"{CreationDate}, {ModificationDate} " +
                   $"{NumOfAdults}, {NumOfChildren}";
        }

        public CartBooking Clone()
        {
            return new CartBooking
            {
                Id = this.Id,
                UserId = this.UserId,
                User = null,
                BookingRooms = null,
                CheckInDate = this.CheckInDate,
                CheckOutDate = this.CheckOutDate,
                SpecialRequests = this.SpecialRequests,
                CreationDate = this.CreationDate,
                ModificationDate = this.ModificationDate,
                NumOfAdults = this.NumOfAdults,
                NumOfChildren = this.NumOfChildren
            };
        }
    }
}
