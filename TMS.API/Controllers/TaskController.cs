using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
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
    private readonly ILogService _logService;

    public TaskController(ITaskService taskService, IJWTService jwtService, ILogService logService)
    {
        _taskService = taskService;
        _jwtService = jwtService;
        _logService = logService;
        _response = new APIResponse();
    }

    [HttpPost("get-tasks")]
    public async Task<IActionResult> GetAllTaskAssign()
    {
        string authToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var (email, role, userId) = _jwtService.ValidateToken(authToken);
        if (string.IsNullOrEmpty(authToken))
        {
            return Unauthorized();
        }
        try
        {
            if (email == null || role == null || userId == null)
                return Unauthorized();
            string? draw = Request.Form["draw"].FirstOrDefault();
            string? start = Request.Form["start"].FirstOrDefault();
            string? length = Request.Form["length"].FirstOrDefault();
            string? searchValue = Request.Form["search[value]"].FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            string? sorting = Request.Form["order[0][column]"].FirstOrDefault();
            string? sortDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            string? taskType = Request.Form["taskType[value]"].FirstOrDefault();
            int? statusFilter = int.Parse(Request.Form["statusFilter[value]"].FirstOrDefault()!);
            int? userFilter = int.Parse(Request.Form["userFilter[value]"].FirstOrDefault()!);

            var (taskList, totalCount) = await _taskService.GetAllTaskAssignAsync(int.Parse(userId), role, taskType, statusFilter, userFilter, skip, pageSize, searchValue, sorting, sortDirection);

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
            UpdateTaskDto? taskAssign = await _taskService.GetTaskAssignAsync(id);
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
        string authToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
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
            if (result.id == 0 && result.message != "Task assigned successfully with recurrence.")
            {
                return BadRequest(result.message);
            }
            string? userId = taskDto.FkUserId.ToString();
            string message = "New Task Assigned!";
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
        string authToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        if (string.IsNullOrEmpty(authToken))
        {
            return Unauthorized();
        }
        var (email, role, userId) = _jwtService.ValidateToken(authToken);
        try
        {
            if (email == null || role == null || userId == null)
                return Unauthorized();

            var (success, message) = await _taskService.UpdateTaskAssignAsync(taskDto, role);
            if (!success)
            {
                return BadRequest(message);
            }
            await _logService.LogAsync("Update task.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Update.ToString(), string.Empty, JsonSerializer.Serialize(taskDto));
            return Ok(message);
        }
        catch (Exception ex)
        {
            await _logService.LogAsync("Update task.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Update.ToString(), ex.StackTrace, JsonSerializer.Serialize(taskDto));
            return StatusCode(500);
        }
    }

    [HttpPost("delete-upcoming/{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> DeleteUpcoming(string id)
    {
        string authToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        if (string.IsNullOrEmpty(authToken))
        {
            return Unauthorized();
        }
        var (email, role, userId) = _jwtService.ValidateToken(authToken);

        try
        {
            if (email == null || role == null || userId == null)
                return Unauthorized();

            var (success, message) = await _taskService.DeleteUpcomingRecurrenceTask(id);
            if (!success)
                return BadRequest(message);
            await _logService.LogAsync("Delete next recurrent tasks.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Update.ToString(), string.Empty, id);
            return Ok(message);
        }
        catch (System.Exception ex)
        {
            await _logService.LogAsync("Delete next recurrent tasks.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Update.ToString(), ex.StackTrace, id);
            return StatusCode(500);
        }
    }

    [HttpPost("delete-recurrence/{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> DeleteRecurrence(string id)
    {
        string authToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        if (string.IsNullOrEmpty(authToken))
        {
            return Unauthorized();
        }
        var (email, role, userId) = _jwtService.ValidateToken(authToken);

        try
        {
            if (email == null || role == null || userId == null)
                return Unauthorized();

            var (success, message) = await _taskService.DeleteRecurrence(id);
            if (!success)
                return BadRequest(message);
            await _logService.LogAsync("Delete entire recurrence.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Update.ToString(), string.Empty, id);
            return Ok(message);
        }
        catch (System.Exception ex)
        {
            await _logService.LogAsync("Delete entire recurrence.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Update.ToString(), ex.StackTrace, id);
            return StatusCode(500);
        }
    }

    [HttpGet("get-tasks")]
    public async Task<IActionResult> GetAllTasks()
    {
        string? userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        try
        {
            List<TaskDto> tasks = await _taskService.GetAllTasksAsync();
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
            List<SubTaskDto> subTasks = await _taskService.GetSubTasksByTaskIdAsync(id);
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
        string authToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
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
        string authToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
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
        string authToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
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
            List<TaskAssignDto> tasks = await _taskService.GetTasksForSchedular(start, end, role, id);
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

    [HttpPost("chart-data")]
    [ProducesResponseType(typeof(List<TaskGraphDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetTaskChartData([FromBody] TaskGraphFilterDto filter)
    {
        string authToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        if (string.IsNullOrEmpty(authToken))
        {
            return Unauthorized();
        }
        var (email, role, userId) = _jwtService.ValidateToken(authToken);
        if (role == "User")
            return Forbid();

        int id = int.Parse(userId!);
        if (email == null || role == null || userId == null)
            return Unauthorized();
        try
        {
            List<TaskGraphDto> chartData = await _taskService.GetTaskChartData(filter);
            await _logService.LogAsync("Get tasks analysis.", id, Repository.Enums.Log.LogEnum.Read.ToString(), string.Empty, JsonSerializer.Serialize(filter));
            return Ok(chartData);
        }
        catch (System.Exception ex)
        {
            await _logService.LogAsync("Get task analysis.", id, Repository.Enums.Log.LogEnum.Exception.ToString(), ex.StackTrace, JsonSerializer.Serialize(filter));
            throw;
        }
    }
}
