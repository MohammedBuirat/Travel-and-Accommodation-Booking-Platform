namespace Travel_and_Accommodation_API.Models
{
    public class Review
    {
        public Guid Id { get; set; }
        public Guid HotelId { get; set; }
        public Hotel Hotel { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public string? Description { get; set; }
        public int OverAllRating { get; set; }
        public DateTimeOffset DateOfReview { get; set; }
        public int? SecurityRating { get; set; }
        public int? StaffRating { get; set; }
        public int? CleanlinessRating { get; set; }
        public int? ValueForMoneyRating { get; set; }
        public int? LocationRating { get; set; }
        public int? AtmosphereRating { get; set; }
        public int? FacilitiesRating { get; set; }

        public override string ToString()
        {
            return $"{Id}, {HotelId}, {UserId}, {Description}, {OverAllRating}, {DateOfReview}, " +
                   $"{SecurityRating}, {StaffRating}, {CleanlinessRating}, {ValueForMoneyRating}, " +
                   $"{LocationRating}, {AtmosphereRating}, {FacilitiesRating}";
        }

        public Review Clone()
        {
            return new Review
            {
                Id = this.Id,
                HotelId = this.HotelId,
                Hotel = null,
                UserId = this.UserId,
                User = null,
                Description = this.Description,
                OverAllRating = this.OverAllRating,
                DateOfReview = this.DateOfReview,
                SecurityRating = this.SecurityRating,
                StaffRating = this.StaffRating,
                CleanlinessRating = this.CleanlinessRating,
                ValueForMoneyRating = this.ValueForMoneyRating,
                LocationRating = this.LocationRating,
                AtmosphereRating = this.AtmosphereRating,
                FacilitiesRating = this.FacilitiesRating
            };
        }
    }
}
