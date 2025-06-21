namespace Travel_and_Accommodation_API.Models
{
    public class BookingRoom
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; }
        public Booking Booking { get; set; }
        public Guid RoomId { get; set; }
        public Room Room { get; set; }

        public override string ToString()
        {
            return $"{Id}, {BookingId}, {RoomId}";
        }

        public BookingRoom Clone()
        {
            return new BookingRoom
            {
                Id = this.Id,
                BookingId = this.BookingId,
                Booking = null,
                RoomId = this.RoomId,
                Room = null
            };
        }
    }

}
