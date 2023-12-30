using AutoMapper;
using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Travel_and_Accommodation_API.Dto.User;
using Travel_and_Accommodation_API.Exceptions_and_logs;
using Travel_and_Accommodation_API.Models;
using Travel_and_Accommodation_API.Services.EmailService;
using Travel_and_Accommodation_API.Services.Validation;

namespace Travel_and_Accommodation_API.Security
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;
        private readonly UserValidation _userValidation;
        private readonly JwtManager _jwtManager;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(
            IMapper mapper,
            UserManager<User> userManager,
            IEmailService emailService,
            UserValidation userValidation,
            JwtManager jwtManager,
            ILogger<AuthenticationService> logger)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _userValidation = userValidation ?? throw new ArgumentNullException(nameof(userValidation));
            _jwtManager = jwtManager ?? throw new ArgumentNullException(nameof(jwtManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> RegisterUser(UserToAdd requestDto)
        {
            try
            {
                var userExist = await _userManager.FindByEmailAsync(requestDto.Email);
                if (userExist != null)
                    throw new ExceptionWithMessage("User registration failed: Email is already in use");

                var userExistByUserName = await _userManager.FindByNameAsync(requestDto.UserName);
                if (userExistByUserName != null)
                    throw new ExceptionWithMessage("User registration failed: UserName is already in use");

                var newUser = _mapper.Map<User>(requestDto);
                newUser.RegistrationDate = DateTime.Now;
                newUser.EmailConfirmed = false;
                var validationResult = await _userValidation.ValidateAsync(newUser);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult);
                }
                var isCreated = await _userManager.CreateAsync(newUser, requestDto.Password);
                if (isCreated.Succeeded)
                {
                    var token = _jwtManager.GenerateJWT(newUser);
                    return token;
                }
                else
                {
                    var errors = isCreated.Errors.Select(e => e.Description);
                    string errosString = string.Join("", errors);
                    throw new ExceptionWithMessage(errosString);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during registration");
                throw;
            }
        }

        public async Task<string> GetUserConfirmationCode(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            return code;
        }

        public async Task SendConfirmationEmail(string url, string email)
        {
            await _emailService.SendEmail(url, "ConfirmEmail", email);
        }

        public async Task<string> LoginUser(AuthenticationRequestBody request)
        {
            try
            {
                var existingsUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingsUser == null)
                    throw new ExceptionWithMessage("User login failed: Invalid Email Or Password");

                var isCorrectPassword = await _userManager.CheckPasswordAsync(existingsUser, request.Password);
                if (!isCorrectPassword)
                    throw new ExceptionWithMessage("User login failed: Invalid Email Or Password");

                var jwtToken = _jwtManager.GenerateJWT(existingsUser);
                return jwtToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Log in failed");
                throw;
            }
        }

        public async Task<IdentityResult?> ConfirmEmail(string email, string code)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    throw new Exception();
                }

                var result = await _userManager.ConfirmEmailAsync(user, code);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during email confirmation");
                throw ex;
            }
        }
    }
}
