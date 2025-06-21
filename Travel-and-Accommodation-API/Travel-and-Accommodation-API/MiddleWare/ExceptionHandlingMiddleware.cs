using Travel_and_Accommodation_API.Exceptions_and_logs;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ElementNotFoundException)
        {
            context.Response.StatusCode = 404;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("Not Found");
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = 422;
            context.Response.ContentType = "application/json";
            var errors = ex.ValidationResult.Errors.Select(e => e.ErrorMessage);
            var result = Newtonsoft.Json.JsonConvert.SerializeObject(new { Errors = errors });
            await context.Response.WriteAsync(result);
        }
        catch (ExceptionWithMessage ex)
        {
            context.Response.StatusCode = 422;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(ex.MessageToReturn);
        }
        catch (UnauthorizedAccessException)
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("Unauthorized Access");
        }
        catch (Exception)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("Internal Server Error");
        }
    }
}
