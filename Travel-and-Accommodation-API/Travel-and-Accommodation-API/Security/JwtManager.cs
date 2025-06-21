using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Travel_and_Accommodation_API.Models;

namespace Travel_and_Accommodation_API.Security
{
    public class JwtManager
    {
        private readonly ILogger<JwtManager> _logger;
        private readonly IConfiguration _configuration;

        public JwtManager(ILogger<JwtManager> logger, IConfiguration configuration)
        {
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ??
                throw new ArgumentNullException(nameof(configuration));
        }

        public string GenerateJWT(User user)
        {
            try
            {

                var jwtTokenHandelr = new JwtSecurityTokenHandler();

                var key = Encoding.UTF8.GetBytes(_configuration["JwtConfig:Secret"]);
                var tokenDiscripter = new SecurityTokenDescriptor()
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim("Id", user.Id),
                        new Claim("Role", "User"),
                        new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                        new Claim(JwtRegisteredClaimNames.Email, user.Email),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToUniversalTime().ToString())
                    }),

                    Expires = DateTime.Now.AddHours(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
                };

                var token = jwtTokenHandelr.CreateToken(tokenDiscripter);
                var jwtToken = jwtTokenHandelr.WriteToken(token);

                _logger.LogInformation("JWT generated successfully for user: {Email}", user.Email);

                return jwtToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during JWT generation");
                throw;
            }
        }
    }
}
