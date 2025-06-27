using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using TMS.API.Hubs;
using TMS.Repository.Data;
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
    private readonly ILogService _logService;

    public TaskController(ITaskService taskService, IJWTService jwtService, IHubContext<NotificationHub> hubContext, ILogService logService)
    {
        _taskService = taskService;
        _jwtService = jwtService;
        _hubContext = hubContext;
        _logService = logService;
        _response = new APIResponse();
    }

    [HttpPost("get-tasks")]
    public async Task<IActionResult> GetAllTaskAssign()
    {
        var authToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var (email, role, userId) = _jwtService.ValidateToken(authToken);
        if (string.IsNullOrEmpty(authToken))
        {
            return Unauthorized();
        }
        try
        {
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

            var (taskList, totalCount) = await _taskService.GetAllTaskAssignAsync(int.Parse(userId), role, skip, pageSize, searchValue, sorting, sortDirection);

            var result = new
            {
                draw = draw,
                recordsFiltered = totalCount,
                recordsTotal = totalCount,
                data = taskList
            };
            await _logService.LogAsync("Get all tasks.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Read.ToString(), string.Empty, string.Empty);
            return Ok(result);
        }
        catch (Exception ex)
        {
            await _logService.LogAsync("Get all tasks.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Exception.ToString(), ex.StackTrace, string.Empty);
            return StatusCode(500);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTaskAssign(int id)
    {
        string? userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        try
        {
            var taskAssign = await _taskService.GetTaskAssignAsync(id);
            if (taskAssign == null)
            {
                return NotFound();
            }
            await _logService.LogAsync("Get task.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Read.ToString(), string.Empty, id.ToString());
            return Ok(taskAssign);
        }
        catch (Exception ex)
        {
            await _logService.LogAsync("Get task.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Read.ToString(), ex.StackTrace, string.Empty);
            return StatusCode(500);
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddTaskAssign([FromBody] AddTaskDto taskDto)
    {
        var authToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        if (string.IsNullOrEmpty(authToken))
        {
            return Unauthorized();
        }
        var (email, role, Id) = _jwtService.ValidateToken(authToken);
        try
        {
            if (email == null || role == null || Id == null)
                return Unauthorized();
            var result = await _taskService.AddTaskAssignAsync(taskDto, role);
            if (result.id == 0)
            {
                return BadRequest(result.message);
            }
            string? userId = taskDto.FkUserId.ToString();
            string message = "New Task Assigned!";
            // await _hubContext.Clients.All.SendAsync("ReceiveNotification", userId, message);
            await _logService.LogAsync("Add task.", int.Parse(Id!), Repository.Enums.Log.LogEnum.Create.ToString(), string.Empty, JsonSerializer.Serialize(taskDto));
            return Created($"/api/tasks/", result.id);
        }
        catch (Exception ex)
        {
            await _logService.LogAsync("Add task.", int.Parse(Id!), Repository.Enums.Log.LogEnum.Create.ToString(), ex.StackTrace, JsonSerializer.Serialize(taskDto));
            return StatusCode(500);
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateTaskAssign([FromBody] EditTaskDto taskDto)
    {
        var authToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        if (string.IsNullOrEmpty(authToken))
        {
            return Unauthorized();
        }
        var (email, role, userId) = _jwtService.ValidateToken(authToken);
        try
        {
            if (email == null || role == null || userId == null)
                return Unauthorized();

            var result = await _taskService.UpdateTaskAssignAsync(taskDto, role);
            if (!result.success)
            {
                return BadRequest(result.message);
            }
            await _logService.LogAsync("Update task.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Update.ToString(), string.Empty, JsonSerializer.Serialize(taskDto));
            return Ok(result.message);
        }
        catch (Exception ex)
        {
            await _logService.LogAsync("Update task.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Update.ToString(), ex.StackTrace, JsonSerializer.Serialize(taskDto));
            return StatusCode(500);
        }
    }

    [HttpGet("get-tasks")]
    public async Task<IActionResult> GetAllTasks()
    {
        string? userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        try
        {
            var tasks = await _taskService.GetAllTasksAsync();
            await _logService.LogAsync("Get task types(master table).", int.Parse(userId!), Repository.Enums.Log.LogEnum.Read.ToString(), string.Empty, string.Empty);
            return Ok(tasks);
        }
        catch (Exception ex)
        {
            await _logService.LogAsync("Get task types(master table).", int.Parse(userId!), Repository.Enums.Log.LogEnum.Read.ToString(), ex.StackTrace, string.Empty);
            return StatusCode(500);
        }
    }

    [HttpGet("get-sub-tasks/{id}")]
    public async Task<IActionResult> GetSubTasksByTaskId(int id)
    {
        string? userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        try
        {
            var subTasks = await _taskService.GetSubTasksByTaskIdAsync(id);
            if (subTasks == null || !subTasks.Any())
            {
                return NotFound();
            }
            await _logService.LogAsync("Get sub tasks(master table).", int.Parse(userId!), Repository.Enums.Log.LogEnum.Read.ToString(), string.Empty, string.Empty);
            return Ok(subTasks);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.ErrorMessage = new List<string> { ex.Message };
            await _logService.LogAsync("Get sub tasks(master table).", int.Parse(userId!), Repository.Enums.Log.LogEnum.Read.ToString(), ex.StackTrace, string.Empty);
            return StatusCode(500, _response);
        }
    }

    [HttpPost("approve/{id}")]
    [ProducesResponseType(typeof(TaskActionDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> ApproveTask(int id)
    {
        var authToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var (email, role, userId) = _jwtService.ValidateToken(authToken);
        try
        {
            if (string.IsNullOrEmpty(authToken))
            {
                return Unauthorized();
            }
            if (email == null || role == null || userId == null)
                return Unauthorized();
            if (role != "Admin")
            {
                return Forbid("Only Admin can approve tasks.");
            }

            TaskAssign result = await _taskService.ApproveTask(id);
            if (result == null)
            {
                return BadRequest("Task approval failed.");
            }
            string? taskUserId = result.FkUserId.ToString();
            string message = "Your Task is Approved !";
            // await _hubContext.Clients.All.SendAsync("ReceiveNotification", taskUserId, message);
            await _logService.LogAsync("Approve task.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Update.ToString(), string.Empty, string.Empty);
            return Ok("Task approved successfully.");
        }
        catch (System.Exception ex)
        {
            await _logService.LogAsync("Approve task.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Update.ToString(), ex.StackTrace, string.Empty);
            return StatusCode(500, "An error occurred while approving the task.");
        }
    }

    [HttpPost("reassign")]
    [ProducesResponseType(typeof(TaskActionDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> ReassignTask([FromForm] ReassignTaskDto dto)
    {
        var authToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var (email, role, userId) = _jwtService.ValidateToken(authToken);
        try
        {
            if (string.IsNullOrEmpty(authToken))
            {
                return Unauthorized();
            }
            if (email == null || role == null || userId == null)
                return Unauthorized();
            if (role != "Admin")
            {
                return Forbid("Only Admin can reassign tasks.");
            }

            TaskAssign result = await _taskService.ReassignTask(dto);
            if (result == null)
            {
                return BadRequest("Task reassignment failed.");
            }
            string? taskUserId = result.FkUserId.ToString();
            string message = "Your Task is Reassigned !";
            // await _hubContext.Clients.All.SendAsync("ReceiveNotification", taskUserId, message);
            await _logService.LogAsync("Reassign tasks.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Update.ToString(), string.Empty, string.Empty);
            return Ok("Task reassigned successfully.");
        }
        catch (System.Exception)
        {
            await _logService.LogAsync("Reassign tasks.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Update.ToString(), string.Empty, string.Empty);
            return StatusCode(500, "An error occurred while reassigning the task.");
        }
    }

    [HttpGet("schedular-data")]
    [ProducesResponseType(typeof(List<TaskAssignDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetTaskForSchedular(DateTime start, DateTime end)
    {
        var authToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        if (string.IsNullOrEmpty(authToken))
        {
            return Unauthorized();
        }
        var (email, role, userId) = _jwtService.ValidateToken(authToken);
        int id = int.Parse(userId!);
        if (email == null || role == null || userId == null)
            return Unauthorized();
        try
        {
            var tasks = await _taskService.GetTasksForSchedular(start, end, role, id);
            if (tasks == null || !tasks.Any())
            {
                return NotFound("No tasks found for the specified date range.");
            }
            await _logService.LogAsync("Get tasks for schedular.", id, Repository.Enums.Log.LogEnum.Read.ToString(), string.Empty, $"{start} to {end}");
            return Ok(tasks);
        }
        catch (Exception ex)
        {
            await _logService.LogAsync("Get tasks for schedular.", id, Repository.Enums.Log.LogEnum.Exception.ToString(), ex.StackTrace, $"{start} to {end}");
            return StatusCode(500, _response);
        }
    }
}
