namespace Travel_and_Accommodation_API.Dto.RoomDay
{
    public class RoomDayDto
    {
        public Guid Id { get; set; }
        public Guid RoomId { get; set; }
        public decimal Price { get; set; }
        public DateTime Date { get; set; }
        public bool Available { get; set; }
    }
}
