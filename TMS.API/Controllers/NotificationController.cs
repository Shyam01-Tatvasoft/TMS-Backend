using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using TMS.Service.Interfaces;

namespace TMS.API.Controllers;

[EnableCors("AllowSpecificOrigin")]
[Route("api/notification")]
[ApiController]
public class NotificationController : Controller
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetNotificationById(int id)
    {
        try
        {
            var notification = await _notificationService.GetNotificationAsync(id);
            if (notification == null)
            {
                return NotFound();
            }
            return Ok(notification);
        }
        catch (System.Exception)
        {
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        try
        {
            string result = await _notificationService.MarkAsRead(id);
                return Ok(result);
        }
        catch (System.Exception)
        {
            return StatusCode(500, "Internal server error");
        }
    }
}
