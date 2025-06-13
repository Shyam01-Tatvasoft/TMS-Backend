using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using TMS.Repository.Data;
using TMS.Repository.Dtos;
using TMS.Service.Interfaces;

namespace TMS.API.Controllers;

[Route("api/user")]
[EnableCors("AllowSpecificOrigin")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly APIResponse _response;
    private readonly IJWTService _jwtService;

    public UserController(IUserService userService,IJWTService jwtService)
    {
        _userService = userService;
        _jwtService = jwtService;
        this._response = new();
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<APIResponse>> GetUsers()
    {
        try
        {
            List<UserDto> userList = await _userService.GetUsers();
            _response.Result =  new { data = userList };
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }
        catch (System.Exception ex)
        {
            _response.StatusCode = HttpStatusCode.InternalServerError;
            _response.ErrorMessage = new List<string> { ex.Message };
            return StatusCode((int)HttpStatusCode.InternalServerError, _response);
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
        }
        catch (System.Exception ex)
        {
            _response.StatusCode = HttpStatusCode.InternalServerError;
            _response.ErrorMessage = new List<string> { ex.Message };
            return StatusCode((int)HttpStatusCode.InternalServerError, _response);
        }
        _response.IsSuccess = true;
        _response.StatusCode = HttpStatusCode.OK;
        return Ok(_response);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<APIResponse>> GetUserById(int id)
    {
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
            return Ok(_response);
        }
        catch (System.Exception ex)
        {
            _response.StatusCode = HttpStatusCode.InternalServerError;
            _response.ErrorMessage = new List<string> { ex.Message };
            return StatusCode((int)HttpStatusCode.InternalServerError, _response);
        }
    }

    [HttpPost]
    public async Task<ActionResult<APIResponse>> AddUser([FromForm] AddEditUserDto userDto)
    {
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
            return Ok(_response);
        }
        catch (System.Exception ex)
        {
            _response.StatusCode = HttpStatusCode.InternalServerError;
            _response.ErrorMessage = new List<string> { ex.Message };
            return StatusCode((int)HttpStatusCode.InternalServerError, _response);
        }
    }


    [HttpPut("{id}")]
    public async Task<ActionResult<APIResponse>> UpdateUser(int id, [FromForm] AddEditUserDto userDto)
    {
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
            return Ok(_response);
        }
        catch (System.Exception ex)
        {
            _response.StatusCode = HttpStatusCode.InternalServerError;
            _response.ErrorMessage = new List<string> { ex.Message };
            return StatusCode((int)HttpStatusCode.InternalServerError, _response);
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<APIResponse>> DeleteUser(int id)
    {
        try
        {
            var (success, message) = await _userService.DeleteUser(id);
            _response.StatusCode = success ? HttpStatusCode.OK : HttpStatusCode.NotFound;
            _response.IsSuccess = success;
            _response.ErrorMessage = success ? null : new List<string> { message };
            _response.Result = success ? message : null;
            return Ok(_response);
        }
        catch (System.Exception ex)
        {
            _response.StatusCode = HttpStatusCode.InternalServerError;
            _response.ErrorMessage = new List<string> { ex.Message };
            return StatusCode((int)HttpStatusCode.InternalServerError, _response);
        }
    }
}
