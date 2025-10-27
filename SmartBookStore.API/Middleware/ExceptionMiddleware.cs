using System.Net;
using System.Text.Json;

namespace SmartBookStore.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            StatusCode = (int)HttpStatusCode.InternalServerError,
            Message = "An error occurred while processing your request",
            Details = exception.Message
        };

        switch (exception)
        {
            case ArgumentNullException:
            case ArgumentException:
                response = new
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "Invalid request parameters",
                    Details = exception.Message
                };
                break;
            case UnauthorizedAccessException:
                response = new
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Unauthorized access",
                    Details = exception.Message
                };
                break;
            case KeyNotFoundException:
                response = new
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Resource not found",
                    Details = exception.Message
                };
                break;
        }

        context.Response.StatusCode = response.StatusCode;
        
        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}