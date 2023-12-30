namespace Travel_and_Accommodation_API.Dto.Review
{
    public class ReviewDto
    {
        public Guid Id { get; set; }
        public Guid HotelId { get; set; }
        public Guid UserId { get; set; }
        public string? Description { get; set; }
        public int OverAllRating { get; set; }
        public DateTimeOffset DateOfReview { get; set; }
        public int SecurityRating { get; set; }
        public int StaffRating { get; set; }
        public int CleanlinessRating { get; set; }
        public int ValueForMoneyRating { get; set; }
        public int LocationRating { get; set; }
        public int AtmosphereRating { get; set; }
        public int FacilitiesRating { get; set; }
    }
}
