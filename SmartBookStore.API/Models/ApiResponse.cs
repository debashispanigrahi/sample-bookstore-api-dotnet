using System.Net;

namespace SmartBookStore.API.Models;

public class ApiResponse
{
    public object? Data { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsSuccess => ErrorMessage == null;
}