using System.Net;

namespace SmartBookStore.API.Models;

public class ApiResponse<T>
{
    public T? Data { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsSuccess => ErrorMessage == null;

    public static ApiResponse<T> Success(T data, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new ApiResponse<T>
        {
            Data = data,
            StatusCode = statusCode
        };
    }

    public static ApiResponse<T> Failure(string errorMessage, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
    {
        return new ApiResponse<T>
        {
            ErrorMessage = errorMessage,
            StatusCode = statusCode
        };
    }
}