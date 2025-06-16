using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using TMS.Repository.Data;
using TMS.Repository.Dtos;
using TMS.Service.Implementations;
using TMS.Service.Interfaces;

namespace TMS.API.Controllers;

[Route("api/dashboard")]
[EnableCors("AllowSpecificOrigin")]
public class DashboardController : ControllerBase
{
    private readonly IJWTService _jwtService;
    private readonly APIResponse _response;
    private readonly IUserService _userService;
    public DashboardController(IJWTService jWTService, IUserService userService)
    {
        _jwtService = jWTService;
        _userService = userService;
        this._response = new();
    }


    [HttpGet]
    public async Task<ActionResult<APIResponse>> Dashboard()
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
            UserDto? user = await _userService.GetUserByEmail(email);
            _response.Result = user;
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
}
