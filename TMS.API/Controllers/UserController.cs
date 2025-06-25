using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using TMS.Repository.Data;
using TMS.Repository.Dtos;
using TMS.Repository.Enums;
using TMS.Service.Interfaces;

namespace TMS.API.Controllers;

[Route("api/user")]
[EnableCors("AllowSpecificOrigin")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly APIResponse _response;
    private readonly IJWTService _jwtService;
    private readonly ILogService _logService;

    public UserController(IUserService userService,IJWTService jwtService,ILogService logService)
    {
        _userService = userService;
        _jwtService = jwtService;
        _logService = logService;
        this._response = new();
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUsers()
    {   
        string? userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        try
        {
            List<UserDto> userList = await _userService.GetUsers();
            await _logService.LogAsync("Get User", int.Parse(userId!), Repository.Enums.Log.LogEnum.Read.ToString(), string.Empty, string.Empty);
            return Ok(userList);
        }
        catch (System.Exception ex)
        {   
            await _logService.LogAsync("Get User", int.Parse(userId!), Repository.Enums.Log.LogEnum.Exception.ToString(), ex.StackTrace, string.Empty);
            return StatusCode(500);
        }
    }

    [HttpPost("get-users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFilteredUsers()
    {
        var authToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
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
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            var sorting = Request.Form["order[0][column]"].FirstOrDefault();
            var sortDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            
            var (userList , totalCount)= await _userService.GetUsers( skip, pageSize, searchValue, sorting, sortDirection);
            var result = new
            {
                draw = draw,
                recordsFiltered = totalCount,
                recordsTotal = totalCount,
                data = userList
            };
            await _logService.LogAsync("Get filtered user", int.Parse(userId!), Repository.Enums.Log.LogEnum.Read.ToString(), string.Empty, string.Empty);
            return Ok(result);
        }
        catch (System.Exception ex)
        {
            await _logService.LogAsync("Get filtered user", int.Parse(userId!), Repository.Enums.Log.LogEnum.Exception.ToString(), ex.StackTrace, string.Empty);
            return StatusCode(500);
        }
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpGet("get-user")]
    public async Task<ActionResult<APIResponse>> GetUser()
    {
        var authToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        if (string.IsNullOrEmpty(authToken))
        {
            _response.StatusCode = HttpStatusCode.Unauthorized;
            _response.IsSuccess = false;
            return Unauthorized(_response);
        }

        var (email, role, userId) = _jwtService.ValidateToken(authToken);
        if (string.IsNullOrEmpty(email))
        {
            _response.StatusCode = HttpStatusCode.Unauthorized;
            _response.IsSuccess = false;
            return Unauthorized(_response);
        }

        try
        {   
            _response.Result = await _userService.GetUserByEmail(email);
            await _logService.LogAsync("Get user", int.Parse(userId!), Repository.Enums.Log.LogEnum.Read.ToString(), string.Empty, string.Empty);
        }
        catch (System.Exception ex)
        {
            _response.StatusCode = HttpStatusCode.InternalServerError;
            _response.ErrorMessage = new List<string> { ex.Message };
            await _logService.LogAsync("Get user", int.Parse(userId!), Repository.Enums.Log.LogEnum.Exception.ToString(), ex.StackTrace, string.Empty);
            return StatusCode((int)HttpStatusCode.InternalServerError, _response);
        }
        _response.IsSuccess = true;
        _response.StatusCode = HttpStatusCode.OK;
        return Ok(_response);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<APIResponse>> GetUserById(int id)
    {
        string? userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        try
        {
            UserDto user = await _userService.GetUserById(id);
            if (user == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.ErrorMessage = new List<string> { "User not found." };
                return NotFound(_response);
            }

            _response.Result = user;
            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            await _logService.LogAsync("Get user", int.Parse(userId!), Repository.Enums.Log.LogEnum.Read.ToString(), string.Empty, id.ToString());
            return Ok(_response);
        }
        catch (System.Exception ex)
        {
            _response.StatusCode = HttpStatusCode.InternalServerError;
            _response.ErrorMessage = new List<string> { ex.Message };
            await _logService.LogAsync("Get user", int.Parse(userId!), Repository.Enums.Log.LogEnum.Exception.ToString(), ex.StackTrace, id.ToString());
            return StatusCode((int)HttpStatusCode.InternalServerError, _response);
        }
    }

    [HttpPost]
    public async Task<ActionResult<APIResponse>> AddUser([FromForm] AddEditUserDto userDto)
    {
        string? userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (!ModelState.IsValid)
        {
            _response.IsSuccess = false;
            _response.ErrorMessage = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(_response);
        }

        try
        {
            var (success, message) = await _userService.AddUser(userDto);
            _response.StatusCode = success ? HttpStatusCode.Created : HttpStatusCode.BadRequest;
            _response.IsSuccess = success;
            _response.ErrorMessage = success ? null : new List<string> { message };
            _response.Result = success ? message : null;

            if(success)
            {
                await _logService.LogAsync("Add User", int.Parse(userId!), Repository.Enums.Log.LogEnum.Create.ToString(), string.Empty, JsonSerializer.Serialize(userDto));
            }
            return Ok(_response);
        }
        catch (System.Exception ex)
        {
            _response.StatusCode = HttpStatusCode.InternalServerError;
            _response.ErrorMessage = new List<string> { ex.Message };
            await _logService.LogAsync("Add User", int.Parse(userId!), Repository.Enums.Log.LogEnum.Exception.ToString(), ex.StackTrace, JsonSerializer.Serialize(userDto));
            return StatusCode((int)HttpStatusCode.InternalServerError, _response);
        }
    }


    [HttpPut("{id}")]
    public async Task<ActionResult<APIResponse>> UpdateUser(int id, [FromForm] AddEditUserDto userDto)
    {
        string? userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (id != userDto.Id)
        {
            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.ErrorMessage = new List<string> { "User ID mismatch." };
            return BadRequest(_response);
        }

        if (!ModelState.IsValid)
        {
            _response.IsSuccess = false;
            _response.ErrorMessage = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(_response);
        }

        try
        {
            var (success, message) = await _userService.UpdateUser(userDto);
            _response.StatusCode = success ? HttpStatusCode.OK : HttpStatusCode.NotFound;
            _response.IsSuccess = success;
            _response.ErrorMessage = success ? null : new List<string> { message };
            _response.Result = success ? message : null;

            if(success)
            {
                await _logService.LogAsync("Update User", int.Parse(userId!), Repository.Enums.Log.LogEnum.Update.ToString(), string.Empty, JsonSerializer.Serialize(userDto));
            }
            return Ok(_response);
        }
        catch (System.Exception ex)
        {
            _response.StatusCode = HttpStatusCode.InternalServerError;
            _response.ErrorMessage = new List<string> { ex.Message };
            await _logService.LogAsync("Update User", int.Parse(userId!), Repository.Enums.Log.LogEnum.Exception.ToString(), ex.StackTrace, JsonSerializer.Serialize(userDto));
            return StatusCode((int)HttpStatusCode.InternalServerError, _response);
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<APIResponse>> DeleteUser(int id)
    {
        string? userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        try
        {
            var (success, message) = await _userService.DeleteUser(id);
            _response.StatusCode = success ? HttpStatusCode.OK : HttpStatusCode.NotFound;
            _response.IsSuccess = success;
            _response.ErrorMessage = success ? null : new List<string> { message };
            _response.Result = success ? message : null;
            await _logService.LogAsync("Delete User", int.Parse(userId!), Repository.Enums.Log.LogEnum.Update.ToString(), string.Empty, id.ToString());
            return Ok(_response);
        }
        catch (System.Exception ex)
        {
            _response.StatusCode = HttpStatusCode.InternalServerError;
            _response.ErrorMessage = new List<string> { ex.Message };
            await _logService.LogAsync("Delete User", int.Parse(userId!), Repository.Enums.Log.LogEnum.Exception.ToString(), ex.StackTrace, id.ToString());
            return StatusCode((int)HttpStatusCode.InternalServerError, _response);
        }
    }
}
