using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using TMS.Repository.Dtos;
using TMS.Service.Interfaces;

namespace TMS.API.Controllers;


[Route("api/system-configuration")]
[ApiController]
[EnableCors("AllowSpecificOrigin")]
[Authorize]
public class SystemConfigurationController : ControllerBase
{
    private readonly ISystemConfigurationService _systemConfigurationService;
    private readonly ILogService _logService;
    private readonly IJWTService _jwtService;
    public SystemConfigurationController(ISystemConfigurationService systemConfigurationService, ILogService logService, IJWTService jwtService)
    {
        _systemConfigurationService = systemConfigurationService;
        _logService = logService;
        _jwtService = jwtService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(SystemConfigurationDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetConfigurations()
    {
        string authToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        if (string.IsNullOrEmpty(authToken))
        {
            return Unauthorized();
        }
        var (email, role, userId) = _jwtService.ValidateToken(authToken);
        if(role != "Admin")
        {
            return Forbid("You do not have permission to access this resource.");
        }
        try
        {
            if (email == null || role == null || userId == null)
            {
                return Unauthorized();
            }
            SystemConfigurationDto systemConfiguration = await _systemConfigurationService.GetAllSystemConfiguration();
            if (systemConfiguration == null)
            {
                return NotFound("No system configurations found.");
            }
            await _logService.LogAsync("System configurations retrieved successfully.", int.Parse(userId ?? "0"), Repository.Enums.Log.LogEnum.Read.ToString(), string.Empty, string.Empty);
            return Ok(systemConfiguration);
        }
        catch (Exception ex)
        {
            await _logService.LogAsync("Error retrieving system configurations.", int.TryParse(userId, out var id) ? id : 0, Repository.Enums.Log.LogEnum.Error.ToString(), ex.StackTrace, string.Empty);
            return StatusCode(500, "An error occurred while retrieving system configurations.");
        }
    }


    [HttpPut]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> UpdateConfigurations([FromBody] List<ConfigurationDto> systemConfigs)
    {
        string authToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        if (string.IsNullOrEmpty(authToken))
        {
            return Unauthorized();
        }
        var (email, role, userId) = _jwtService.ValidateToken(authToken);
        if (role != "Admin")
        {
            return Forbid("You do not have permission to access this resource.");
        }
        try
        {
            if (email == null || role == null || userId == null)
            {
                return Unauthorized();
            }
            await _systemConfigurationService.UpdateSystemConfiguration(systemConfigs);
            await _logService.LogAsync("System configurations updated successfully.", int.Parse(userId), Repository.Enums.Log.LogEnum.Update.ToString(), string.Empty, string.Empty);
            return Ok("System configurations updated successfully.");
        }
        catch (Exception ex)
        {
            await _logService.LogAsync("Error updating system configurations.", int.TryParse(userId, out var id) ? id : 0, Repository.Enums.Log.LogEnum.Error.ToString(), ex.StackTrace, string.Empty);
            return StatusCode(500, "An error occurred while updating system configurations.");
        }
    }

    [HttpGet("result")]
    public Task<IActionResult> GetResult()
    {
        return Task.FromResult<IActionResult>(Ok("Result from GetResult method"));
    }
}
