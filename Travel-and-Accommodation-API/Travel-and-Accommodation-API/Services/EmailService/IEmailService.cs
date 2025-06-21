namespace Travel_and_Accommodation_API.Services.EmailService
{
    public interface IEmailService
    {
        public Task SendEmail(string body, string subject, string email);
    }
}
