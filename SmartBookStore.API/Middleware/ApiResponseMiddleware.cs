using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using SmartBookStore.API.Models;

namespace SmartBookStore.API.Middleware;

public class ApiResponseMiddleware
{
    private readonly RequestDelegate _next;

    public ApiResponseMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var originalBodyStream = context.Response.Body;

        try
        {
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            await _next(context);

            memoryStream.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();
            memoryStream.Seek(0, SeekOrigin.Begin);

            if (context.Response.StatusCode == (int)HttpStatusCode.OK)
            {
                var result = System.Text.Json.JsonSerializer.Deserialize<object>(responseBody);
                var apiResponse = ApiResponse<object>.Success(result);
                context.Response.Body = originalBodyStream;
                await context.Response.WriteAsJsonAsync(apiResponse);
            }
            else
            {
                var apiResponse = ApiResponse<object>.Failure(responseBody, (HttpStatusCode)context.Response.StatusCode);
                context.Response.Body = originalBodyStream;
                await context.Response.WriteAsJsonAsync(apiResponse);
            }
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }
}

public static class ApiResponseMiddlewareExtensions
{
    public static IApplicationBuilder UseApiResponseMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ApiResponseMiddleware>();
    }
}