using Microsoft.AspNetCore.Mvc;
using Travel_and_Accommodation_API.Dto.User;
using Travel_and_Accommodation_API.Security;

namespace Travel_and_Accommodation_API.Controllers
{
    [Route("api/v1.0/")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly ILogger<AuthenticationController> _logger;
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationController(
            ILogger<AuthenticationController> logger,
            IAuthenticationService authenticationService)
        {
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
            _authenticationService = authenticationService ??
                throw new ArgumentNullException(nameof(authenticationService));
        }

        [HttpPost]
        [Route("Register")]
        public async Task<ActionResult> Register(UserToAdd requestDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
                }

                var token = await _authenticationService.RegisterUser(requestDto);
                var code = await _authenticationService.GetUserConfirmationCode(requestDto.Email);
                var link = Url.Action(nameof(ConfirmEmail), "Authentication", new { code, email = requestDto.Email }, Request.Scheme);
                await _authenticationService.SendConfirmationEmail(link, requestDto.Email);
                return Ok(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during user registration");
                throw;
            }
        }

        [HttpPost]
        [Route("Login")]
        public async Task<ActionResult<string>> Login(AuthenticationRequestBody request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogError("User login failed: Email and Password are required");
                    return BadRequest("Email and Password are required");
                }

                var jwtToken = await _authenticationService.LoginUser(request);

                return Ok(jwtToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during user login");
                throw;
            }
        }

        [HttpGet]
        [Route("ConfirmEmail")]
        public async Task<ActionResult> ConfirmEmail(string email, string code)
        {
            try
            {
                if (email == null || code == null)
                {
                    _logger.LogError("Email confirmation failed: Invalid parameters");
                    return BadRequest();
                }
                var result = await _authenticationService.ConfirmEmail(email, code);

                if (result?.Succeeded == true)
                {
                    _logger.LogInformation("Email confirmation successful for user: {email}", email);
                    return Ok("Thank you for confirming your email");
                }
                else
                {
                    _logger.LogError("Email confirmation failed for user: {email}", email);
                    return BadRequest("Your email is not confirmed. Please try again later.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during email confirmation");
                throw;
            }
        }
    }
}
