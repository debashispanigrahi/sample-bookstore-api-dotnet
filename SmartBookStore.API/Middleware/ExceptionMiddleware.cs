using System.Net;
using SmartBookStore.API.Models;

namespace SmartBookStore.API.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            ArgumentNullException or ArgumentException => (HttpStatusCode.BadRequest, "Invalid request parameters."),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized access."),
            KeyNotFoundException => (HttpStatusCode.NotFound, "Resource not found."),
            _ => (HttpStatusCode.InternalServerError, "An error occurred while processing your request.")
        };

        var response = new ApiResponse
        {
            StatusCode = statusCode,
            ErrorMessage = message
        };

        IResult result = statusCode switch
        {
            HttpStatusCode.BadRequest => TypedResults.BadRequest(response),
            HttpStatusCode.Unauthorized => TypedResults.Unauthorized(),
            HttpStatusCode.NotFound => TypedResults.NotFound(response),
            _ => TypedResults.InternalServerError(response)
        };

        // Execute the result asynchronously
        await result.ExecuteAsync(context);
    }
}

public static class ExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionMiddleware>();
    }
}