using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using TMS.Repository.Dtos;
using TMS.Service.Interfaces;

namespace TMS.API.Controllers;

[Route("api/task-action")]
[ApiController]
[EnableCors("AllowSpecificOrigin")]
public class TaskActionController : ControllerBase
{
    private readonly ITaskActionService _taskActionService;

    public TaskActionController(ITaskActionService taskActionService)
    {
        _taskActionService = taskActionService;
    }

    [HttpPost("send-email")]
    public async Task<IActionResult> SendEmail([FromBody] EmailTaskDto dto)
    {
        try
        {
            if (dto == null || string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Subject))
            {
                return BadRequest("Invalid email task data.");
            }

            var taskAction = await _taskActionService.AddTaskActionAsync(dto);
            if (taskAction == null)
            {
                return StatusCode(500, "Failed to add task action.");
            }

            return Ok(taskAction);
        }
        catch (System.Exception)
        {
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpPost("upload-file")]
    public async Task<IActionResult> UploadFiles([FromForm] UploadFileTaskDto dto)
    {
        var files = dto.Files;

        if (files.Count == 0)
            return BadRequest("No files received.");

        try
        {
            int taskAction = await _taskActionService.AddUploadTaskAsync(dto);
            if (taskAction == null)
            {
                return StatusCode(500, "Failed to add task action.");
            }

            return Ok(taskAction);
        }
        catch (System.Exception)
        {
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }
}
