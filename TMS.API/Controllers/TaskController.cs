using System.Net;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
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

    public TaskController(ITaskService taskService, IJWTService jwtService)
    {
        _taskService = taskService;
        _jwtService = jwtService;
        _response = new APIResponse();
    }

    [HttpGet]
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
            var taskAssigns = await _taskService.GetAllTaskAssignAsync(int.Parse(userId),role);
            return Ok(taskAssigns);
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
            return Created($"/api/tasks/",result.id);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateTaskAssign([FromBody] EditTaskDto taskDto)
    {
        // if (!ModelState.IsValid)
        // {
        //     _response.IsSuccess = false;
        //     _response.ErrorMessage = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        //     _response.StatusCode = HttpStatusCode.BadRequest;
        //     return BadRequest(_response);
        // }

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
