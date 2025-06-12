using System.Net;
using Microsoft.AspNetCore.Mvc;
using TMS.Repository.Dtos;
using TMS.Service.Interfaces;

namespace TMS.API.Controllers;

[Route("api/tasks")]
[ApiController]
public class TaskController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly APIResponse _response;

    public TaskController(ITaskService taskService)
    {
        _taskService = taskService;
        _response = new APIResponse();
    }

    [HttpGet]
    public async Task<ActionResult<APIResponse>> GetAllTaskAssign()
    {
        try
        {
            var taskAssigns = await _taskService.GetAllTaskAssignAsync();
            _response.Result = taskAssigns;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.ErrorMessage = new List<string> { ex.Message };
            return StatusCode(500, _response);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<APIResponse>> GetTaskAssign(int id)
    {
        try
        {
            var taskAssign = await _taskService.GetTaskAssignAsync(id);
            if (taskAssign == null)
            {
                _response.IsSuccess = false;
                _response.ErrorMessage = new List<string> { "Task Assign not found" };
                return NotFound(_response);
            }
            _response.Result = taskAssign;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.ErrorMessage = new List<string> { ex.Message };
            return StatusCode(500, _response);
        }
    }

    [HttpPost]
    public async Task<ActionResult<APIResponse>> AddTaskAssign([FromBody] AddEditTaskDto taskDto)
    {
        if (!ModelState.IsValid)
        {
            _response.IsSuccess = false;
            _response.ErrorMessage = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            _response.StatusCode = HttpStatusCode.BadRequest;
            return BadRequest(_response);
        }

        try
        {
            var result = await _taskService.AddTaskAssignAsync(taskDto);
            if (!result.success)
            {
                _response.IsSuccess = false;
                _response.ErrorMessage = new List<string> { result.message };
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.Created;
            _response.Result = result.message;
            return CreatedAtAction(nameof(GetTaskAssign), new { id = taskDto.Id }, _response);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.ErrorMessage = new List<string> { ex.Message };
            return StatusCode(500, _response);
        }
    }

    [HttpPut]
    public async Task<ActionResult<APIResponse>> UpdateTaskAssign([FromBody] AddEditTaskDto taskDto)
    {
        if (!ModelState.IsValid)
        {
            _response.IsSuccess = false;
            _response.ErrorMessage = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            _response.StatusCode = HttpStatusCode.BadRequest;
            return BadRequest(_response);
        }

        try
        {
            var result = await _taskService.UpdateTaskAssignAsync(taskDto);
            if (!result.success)
            {
                _response.IsSuccess = false;
                _response.ErrorMessage = new List<string> { result.message };
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = result.message;
            return Ok(_response);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.ErrorMessage = new List<string> { ex.Message };
            return StatusCode(500, _response);
        }
    }

    [HttpGet("get-tasks")]
    public async Task<ActionResult<APIResponse>> GetAllTasks()
    {
        try
        {
            var tasks = await _taskService.GetAllTasksAsync();
            _response.Result = tasks;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.ErrorMessage = new List<string> { ex.Message };
            return StatusCode(500, _response);
        }
    }

    [HttpGet("get-sub-tasks/{id}")]
    public async Task<ActionResult<APIResponse>> GetSubTasksByTaskId(int id)
    {
        try
        {
            var subTasks = await _taskService.GetSubTasksByTaskIdAsync(id);
            if (subTasks == null || !subTasks.Any())
            {
                _response.IsSuccess = false;
                _response.ErrorMessage = new List<string> { "No sub-tasks found for this task." };
                _response.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_response);
            }
            _response.Result = subTasks;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.ErrorMessage = new List<string> { ex.Message };
            return StatusCode(500, _response);
        }
    }
    
}
