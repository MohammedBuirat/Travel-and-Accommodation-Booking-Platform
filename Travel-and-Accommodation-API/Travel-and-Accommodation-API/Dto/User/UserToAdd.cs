using System.ComponentModel.DataAnnotations;

namespace Travel_and_Accommodation_API.Dto.User
{
    public class UserToAdd
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public Guid CountryId { get; set; }
        [Required]
        public DateTime BirthDate { get; set; }
    }
}
