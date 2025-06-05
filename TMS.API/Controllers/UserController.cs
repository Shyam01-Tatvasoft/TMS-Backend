using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using TMS.Repository.Data;
using TMS.Repository.Dtos;
using TMS.Service.Interfaces;

namespace TMS.API.Controllers;

[Route("api/[controller]")]
[EnableCors("AllowSpecificOrigin")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly APIResponse _response;

    public UserController(IUserService userService)
    {
        _userService = userService;
        this._response = new();
    }

    // [Authorize]
    [HttpGet("GatUsers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<APIResponse>> GetUsers()
    {
        try
        {
            List<User> userList = await _userService.GetUsers();
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

}
