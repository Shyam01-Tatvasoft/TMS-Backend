using System.Net;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TMS.API.Hubs;
using TMS.Repository.Dtos;
using TMS.Service.Interfaces;

namespace TMS.API.Controllers;

[Route("api/tasks")]
[EnableCors("AllowSpecificOrigin")]
[ApiController]
public class TaskController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly APIResponse _response;
    private readonly IJWTService _jwtService;
    private readonly IHubContext<NotificationHub> _hubContext;

    public TaskController(ITaskService taskService, IJWTService jwtService, IHubContext<NotificationHub> hubContext)
    {
        _taskService = taskService;
        _jwtService = jwtService;
        _hubContext = hubContext;
        _response = new APIResponse();
    }

    [HttpPost("get-tasks")]
    public async Task<IActionResult> GetAllTaskAssign()
    {
        var authToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        if (string.IsNullOrEmpty(authToken))
        {
            return Unauthorized();
        }
        try
        {
            var (email, role, userId) = _jwtService.ValidateToken(authToken);
            if (email == null || role == null || userId == null)
                return Unauthorized();
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            var sorting = Request.Form["order[0][column]"].FirstOrDefault();
            var sortDirection = Request.Form["order[0][dir]"].FirstOrDefault();

            var (taskList, totalCount) = await _taskService.GetAllTaskAssignAsync(int.Parse(userId), role, skip, pageSize, searchValue,sorting, sortDirection);

            var result = new
            {
                draw = draw,
                recordsFiltered = totalCount,
                recordsTotal = totalCount,
                data = taskList
            };
            return Ok(result);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTaskAssign(int id)
    {
        try
        {
            var taskAssign = await _taskService.GetTaskAssignAsync(id);
            if (taskAssign == null)
            {
                return NotFound();
            }
            return Ok(taskAssign);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddTaskAssign([FromBody] AddTaskDto taskDto)
    {
        try
        {
            var result = await _taskService.AddTaskAssignAsync(taskDto);
            if (result.id == 0)
            {
                return BadRequest(result.message);
            }
            string? userId = taskDto.FkUserId.ToString();
            string message = "New Task Assigned!";
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", userId , message);
            return Created($"/api/tasks/", result.id);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateTaskAssign([FromBody] EditTaskDto taskDto)
    {
        try
        {
            var result = await _taskService.UpdateTaskAssignAsync(taskDto);
            if (!result.success)
            {
                return BadRequest(result.message);
            }
            return Ok(result.message);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("get-tasks")]
    public async Task<IActionResult> GetAllTasks()
    {
        try
        {
            var tasks = await _taskService.GetAllTasksAsync();
            return Ok(tasks);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("get-sub-tasks/{id}")]
    public async Task<IActionResult> GetSubTasksByTaskId(int id)
    {
        try
        {
            var subTasks = await _taskService.GetSubTasksByTaskIdAsync(id);
            if (subTasks == null || !subTasks.Any())
            {
                return NotFound();
            }
            return Ok(subTasks);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.ErrorMessage = new List<string> { ex.Message };
            return StatusCode(500, _response);
        }
    }

}
