using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Travel_and_Accommodation_API.Dto.User;

namespace Travel_and_Accommodation_API.Security
{
    public interface IAuthenticationService
    {
        public Task<string> RegisterUser(UserToAdd requestDto);
        public Task<string> LoginUser(AuthenticationRequestBody request);
        public Task<IdentityResult?> ConfirmEmail(string email, string code);
        public Task<string> GetUserConfirmationCode(string email);
        public Task SendConfirmationEmail(string url, string email);
    }
}
