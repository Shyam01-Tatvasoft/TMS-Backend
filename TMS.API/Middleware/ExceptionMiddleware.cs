using System.Net;
using System.Security.Claims;
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

    public async Task InvokeAsync(HttpContext httpContext, ILogService logService)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex, logService);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception, ILogService logService)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        string? userId = context.User.Claims
             .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        await logService.LogAsync("System Exception", null, Repository.Enums.Log.LogEnum.Exception.ToString(), exception.StackTrace, null);
        

        var result = JsonSerializer.Serialize(new
        {
            StatusCode = context.Response.StatusCode,
            Message = "An unexpected error occurred!"
        });

        await context.Response.WriteAsync(result);
    }
}
