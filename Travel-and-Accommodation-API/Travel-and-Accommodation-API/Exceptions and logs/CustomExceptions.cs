using FluentValidation.Results;

namespace Travel_and_Accommodation_API.Exceptions_and_logs
{
    public class ValidationException : Exception
    {
        public ValidationResult ValidationResult { get; set; }
        public ValidationException(ValidationResult validationResult)
        {
            ValidationResult = validationResult;
        }
    }

    public class ElementNotFoundException : Exception
    {

    }

    public class UnauthorizedException : Exception
    {

    }

    public class ExceptionWithMessage : Exception
    {
        public string MessageToReturn { get; set; }
        public ExceptionWithMessage(string message)
        {
            MessageToReturn = message;
        }
    }
}