using Microsoft.AspNetCore.Identity;

namespace Travel_and_Accommodation_API.Models
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Guid CountryId { get; set; }
        public Country Country { get; set; }
        public bool Active { get; set; }
        public DateTime BirthDate { get; set; }
        public DateTimeOffset RegistrationDate { get; set; }
        public List<Booking> Bookings { get; set; }
        public List<Payment> Payments { get; set; }
        public List<Review> Reviews { get; set; }
        public List<CartBooking> CartBookings { get; set; }
        public List<UserLastHotels> LastHotels { get; set; }

        public override string ToString()
        {
            return $"{Id}, {UserName}, {FirstName}, {LastName}, {CountryId}, {Active}, " +
                   $"{BirthDate}, {RegistrationDate}";
        }

        public User Clone()
        {
            return new User
            {
                Id = this.Id,
                UserName = this.UserName,
                FirstName = this.FirstName,
                LastName = this.LastName,
                CountryId = this.CountryId,
                Country = null,
                Active = this.Active,
                BirthDate = this.BirthDate,
                RegistrationDate = this.RegistrationDate
            };
        }
    }
}
