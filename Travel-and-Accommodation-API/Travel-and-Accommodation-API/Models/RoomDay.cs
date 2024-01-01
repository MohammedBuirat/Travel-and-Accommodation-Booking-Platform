namespace Travel_and_Accommodation_API.Models
{
    public class RoomDay
    {
        public Guid Id { get; set; }
        public Guid RoomId { get; set; }
        public Room Room { get; set; }
        public decimal Price { get; set; }
        public DateTime Date { get; set; }
        public bool Available { get; set; }

        public override string ToString()
        {
            return $"{Id}, {RoomId}, {Price}, {Date}, {Available}";
        }

        public RoomDay Clone()
        {
            return new RoomDay
            {
                Id = this.Id,
                RoomId = this.RoomId,
                Room = null,
                Price = this.Price,
                Date = this.Date,
                Available = this.Available
            };
        }
    }
}
