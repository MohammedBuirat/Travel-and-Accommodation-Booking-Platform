using System.ComponentModel.DataAnnotations;

namespace Travel_and_Accommodation_API.Dto.User
{
    public class AuthenticationRequestBody
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
