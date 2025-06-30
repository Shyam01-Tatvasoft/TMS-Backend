using System.Security.Claims;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using TMS.Repository.Dtos;
using TMS.Service.Interfaces;

namespace TMS.API.Controllers;

[Route("api/notification")]
[EnableCors("AllowSpecificOrigin")]
[ApiController]
public class NotificationController : Controller
{
    private readonly INotificationService _notificationService;
    private ILogService _logService;
    public NotificationController(INotificationService notificationService,ILogService logService)
    {
        _notificationService = notificationService;
        _logService = logService;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetNotificationById(int id)
    {
        string? userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        try
        {
            List<NotificationDto> notification = await _notificationService.GetNotificationAsync(id);
            if (notification == null)
            {
                return NotFound();
            }
            await _logService.LogAsync("Get Notifications.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Read.ToString(), string.Empty, string.Empty);
            return Ok(notification);
        }
        catch (System.Exception ex)
        {
            await _logService.LogAsync("Get Notifications.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Exception.ToString(), ex.StackTrace, id.ToString());
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        string? userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        try
        {
            string result = await _notificationService.MarkAsRead(id);
            await _logService.LogAsync("Mark as read notification.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Update.ToString(), string.Empty, id.ToString());
            return Ok(result);
        }
        catch (System.Exception ex)
        {
            await _logService.LogAsync("Mark as read notification.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Read.ToString(), ex.StackTrace, id.ToString());
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("mark-all-read/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MarkAllAsRead(int userId)
    {
        string? userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        try
        {
            string result = await _notificationService.MarkAllAsRead(userId);
            await _logService.LogAsync("Mark all notifications as read.", int.Parse(userIdClaim!), Repository.Enums.Log.LogEnum.Update.ToString(), string.Empty, userId.ToString());
            return Ok(result);
        }
        catch (System.Exception ex)
        {
            await _logService.LogAsync("Mark all notifications as read.", int.Parse(userIdClaim!), Repository.Enums.Log.LogEnum.Exception.ToString(), ex.StackTrace, userId.ToString());
            return StatusCode(500, "Internal server error");
        }
    }
}
