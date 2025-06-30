using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using TMS.Repository.Dtos;
using TMS.Service.Interfaces;

namespace TMS.API.Controllers;

[Route("api/log")]
[ApiController]
[EnableCors("AllowSpecificOrigin")]
public class LogController : ControllerBase
{

    private readonly IJWTService _jwtService;
    private readonly ILogService _logService;
    public LogController(IJWTService jwtService, ILogService logService)
    {
        _jwtService = jwtService;
        _logService = logService;
    }

    [HttpPost]
    public async Task<IActionResult> GetAllLogs()
    {
       string authToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var (email, role, userId) = _jwtService.ValidateToken(authToken);
        if (string.IsNullOrEmpty(authToken))
        {
            return Unauthorized();
        }
        if(role != "Admin")
        {
            return Forbid();
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
            string? filterBy = Request.Form["filterBy[value]"].FirstOrDefault();

            var (logs, count) = await _logService.GetAllLogsAsync(skip, pageSize, searchValue, sorting, sortDirection, filterBy ?? string.Empty);

            var result = new
            {
                draw = draw,
                recordsTotal = count,
                recordsFiltered = count,
                data = logs
            };
            await _logService.LogAsync("Get all logs.", int.Parse(userId), Repository.Enums.Log.LogEnum.Read.ToString(), string.Empty, string.Empty);
            return Ok(result);
        }
        catch (System.Exception ex)
        {
            await _logService.LogAsync("Get all logs failed.", 0, Repository.Enums.Log.LogEnum.Exception.ToString(), ex.StackTrace, string.Empty);
            throw;
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLogById(int id)
    {
        string authToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var (email, role, userId) = _jwtService.ValidateToken(authToken);
        if (string.IsNullOrEmpty(authToken))
        {
            return Unauthorized();
        }
        if(role != "Admin")
        {
            return Forbid();
        }
        try
        {
            LogDto? log = await _logService.GetLogByIdAsync(id);
            if (log == null)
            {
                return NotFound();
            }
            await _logService.LogAsync("Get log by id.", int.Parse(userId), Repository.Enums.Log.LogEnum.Read.ToString(), string.Empty, id.ToString());
            return Ok(log);
        }
        catch (System.Exception ex)
        {
            await _logService.LogAsync("Get log by id failed.", 0, Repository.Enums.Log.LogEnum.Exception.ToString(), ex.StackTrace, id.ToString());
            return StatusCode(500, "Internal server error");
        }
    }
}
