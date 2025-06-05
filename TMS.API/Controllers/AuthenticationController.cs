using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using TMS.Repository.Dtos;
using TMS.Service.Interfaces;

namespace TMS.API.Controllers;

[Route("api/[controller]")]
[EnableCors("AllowSpecificOrigin")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _autService;
    private readonly IJWTService _jwtService;   
    private readonly APIResponse _response;

    public AuthenticationController(IAuthenticationService autService,IJWTService jwtService)
    {
        _autService = autService;
        _jwtService = jwtService;
        this._response = new();
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<APIResponse>> Register([FromBody] UserRegisterDto dto)
    {
         if (!ModelState.IsValid)
            return BadRequest(ModelState);
        var result = await _autService.RegisterAsync(dto);
        if (result == "Email already registered.")
        {
            ModelState.AddModelError("CustomeError", "Villa Already Exist!");
            _response.IsSuccess = false;
            _response.ErrorMessage[0] = "User Already Exist";
            return BadRequest(_response);
        }
        _response.IsSuccess = true;
        return Ok(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
    {
        var user = await _autService.LoginAsync(dto);
        if (user == null)
        {
            _response.IsSuccess = false;
            _response.ErrorMessage = new List<string> { "InValid Credentials" };;
            return Ok(_response);
        }
        var token = await _jwtService.GenerateToken(user.Email.ToString(), dto.RememberMe);
        var cookieOptions = new CookieOptions
        {
            // HttpOnly = true,
            Secure = true, 
            Expires = dto.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(1)
        };
        Response.Cookies.Append("jwt", token, cookieOptions);
        _response.StatusCode = System.Net.HttpStatusCode.OK;
        _response.IsSuccess = true;
        _response.Result = new { token, loginUser = user };
        return Ok(_response);
    }
}
