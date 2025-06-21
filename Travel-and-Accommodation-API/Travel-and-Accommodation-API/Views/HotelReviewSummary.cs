namespace Travel_and_Accommodation_API.Views
{
    public class HotelReviewSummary
    {
        public Guid HotelId { get; set; }
        public int NumOfReviews { get; set; }
        public decimal OverAllRatings { get; set; }
        public decimal SecurityRating { get; set; }
        public decimal StaffRating { get; set; }
        public decimal CleanlinessRating { get; set; }
        public decimal ValueForMoneyRating { get; set; }
        public decimal LocationRating { get; set; }
        public decimal AtmosphereRating { get; set; }
        public decimal FacilitiesRating { get; set; }
    }
}
