using System.Net;
using System.Text.Json;
using TMS.Service.Interfaces;

namespace TMS.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    // private readonly ILogService _logService;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        // Log to database
        // await _logService.LogExceptionAsync(new ExceptionLog
        // {
        //     Message = exception.Message,
        //     StackTrace = exception.StackTrace,
        // });
        Console.WriteLine(exception.Message);

        var result = JsonSerializer.Serialize(new
        {
            StatusCode = context.Response.StatusCode,
            Message = "An unexpected error occurred!"
        });

        await context.Response.WriteAsync(result);
    }
}
