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
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _autService;
    private readonly IJWTService _jwtService;
    private readonly ICountryService _countryService;
    private readonly APIResponse _response;

    public AuthenticationController(IAuthenticationService autService, IJWTService jwtService, ICountryService countryService)
    {
        _autService = autService;
        _jwtService = jwtService;
        _countryService = countryService;
        this._response = new();
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<APIResponse>> Login([FromBody] UserLoginDto dto)
    {
        if (!ModelState.IsValid)
        {
            _response.IsSuccess = false;
            _response.ErrorMessage = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(_response);
        }
        var user = await _autService.LoginAsync(dto);
        if (user == null)
        {
            _response.IsSuccess = false;
            _response.ErrorMessage = new List<string> { "Invalid Credentials" };
            return Ok(_response);
        }
        var token = await _jwtService.GenerateToken(user.Email.ToString(), dto.RememberMe);

        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
        _response.StatusCode = System.Net.HttpStatusCode.OK;
        _response.IsSuccess = true;
        _response.Result = new { token, loginUser = user };
        return Ok(_response);
    }


    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<APIResponse>> Register([FromBody] UserRegisterDto dto)
    {
        if (!ModelState.IsValid)
        {
            _response.IsSuccess = false;
            _response.ErrorMessage = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(_response);
        }
        var result = await _autService.RegisterAsync(dto);
        if (result == "Email already registered.")
        {
            _response.IsSuccess = false;
            _response.ErrorMessage = new List<string> { result }; ;
            return BadRequest(_response);
        }
        else if (result == "Username already exist.")
        {
            _response.IsSuccess = false;
            _response.ErrorMessage = new List<string> { result };
            return BadRequest(_response);
        }
        _response.StatusCode = HttpStatusCode.OK;
        _response.IsSuccess = true;
        _response.Result = "Registered Successfully";
        return Ok(_response);
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<APIResponse> Logout()
    {
        Response.Cookies.Delete("AuthToken");
        _response.IsSuccess = true;
        _response.Result = "Logged out successfully.";
        return Ok(_response);
    }

    [HttpGet("countries")]
    public async Task<ActionResult<APIResponse>> GetCountries()
    {
        try
        {
            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Result = await _countryService.GetCountries();
            return Ok(_response);
        }
        catch (System.Exception ex)
        {
            _response.IsSuccess = false;
            _response.StatusCode = HttpStatusCode.InternalServerError;
            _response.ErrorMessage = new List<string> { ex.Message };
            return StatusCode((int)HttpStatusCode.InternalServerError, _response);
        }
    }

    [HttpGet("timezone/{id:int}")]
    public async Task<ActionResult<APIResponse>> GetTimezone(int id)
    {
        try
        {
            List<Timezone> timezones = await _countryService.GetTimezonesByCountryId(id);
            if (timezones == null)
            {
                _response.ErrorMessage = new List<string> { "No timezone found" };
                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = false;
                return BadRequest(_response);
            }
            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Result = timezones;
            return Ok(_response);
        }
        catch (System.Exception ex)
        {
            _response.IsSuccess = false;
            _response.StatusCode = HttpStatusCode.InternalServerError;
            _response.ErrorMessage = new List<string> { ex.Message };
            return StatusCode((int)HttpStatusCode.InternalServerError, _response);
        }
    }


    [HttpGet("reset-password")]
    public async Task<ActionResult<APIResponse>> ResetPassword(string token)
    {
        string? email = await _autService.ValidateResetToken(token);
        if (string.IsNullOrEmpty(token))
        {
            _response.ErrorMessage = new List<String> { "Invalid token" };
            _response.IsSuccess = false;
            _response.StatusCode = HttpStatusCode.BadRequest;
            return BadRequest(_response);
        }
        if (string.IsNullOrEmpty(email))
        {
             _response.ErrorMessage = new List<String> { "Invalid token" };
            _response.IsSuccess = false;
            _response.StatusCode = HttpStatusCode.BadRequest;
            return BadRequest(_response);
        }
        _response.StatusCode = HttpStatusCode.OK;
        _response.IsSuccess = true;
        _response.Result = "Token is valid";
        return Ok(_response);
    }


    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<APIResponse>> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        if (!ModelState.IsValid)
        {
            _response.IsSuccess = false;
            _response.ErrorMessage = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(_response);
        }

        var email = await _autService.ValidateResetToken(dto.Token);
        if (string.IsNullOrEmpty(email))
        {
            _response.IsSuccess = false;
            _response.ErrorMessage = new List<string> { "Your token is expired" };
            return BadRequest(_response);
        }

        var result = await _autService.ResetPasswordAsync(email, dto);
        if (!result)
        {
            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.IsSuccess = false;
            _response.ErrorMessage = new List<string> { "Failed to reset password" };
            return BadRequest(_response);
        }

        _response.StatusCode = HttpStatusCode.OK;
        _response.Result = "Password reset successfully";
        return Ok(_response);
    }

}
