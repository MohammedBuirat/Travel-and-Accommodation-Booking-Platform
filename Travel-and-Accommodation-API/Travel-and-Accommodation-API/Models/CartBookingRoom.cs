namespace Travel_and_Accommodation_API.Models
{
    public class CartBookingRoom
    {
        public Guid Id { get; set; }
        public Guid RoomId { get; set; }
        public Room Room { get; set; }
        public Guid CartBookingId { get; set; }
        public CartBooking CartBooking { get; set;}

        public override string ToString()
        {
            return $"{Id}, {RoomId}, {CartBookingId}";
        }

        public CartBookingRoom Clone()
        {
            return new CartBookingRoom
            {
                Id = this.Id,
                RoomId = this.RoomId,
                Room = null,
                CartBookingId = this.CartBookingId,
                CartBooking = null
            };
        }
    }
}


