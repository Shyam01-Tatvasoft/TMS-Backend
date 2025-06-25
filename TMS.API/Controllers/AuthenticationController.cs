using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using TMS.Repository.Data;
using TMS.Repository.Dtos;
using TMS.Service.Interfaces;

namespace TMS.API.Controllers;

[Route("api/authentication")]
[EnableCors("AllowSpecificOrigin")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _autService;
    private readonly IJWTService _jwtService;
    private readonly ICountryService _countryService;
    private readonly APIResponse _response;
    private readonly ILogService _logService;

    public AuthenticationController(IAuthenticationService autService, IJWTService jwtService, ICountryService countryService, ILogService logService)
    {
        _autService = autService;
        _jwtService = jwtService;
        _countryService = countryService;
        _logService = logService;
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
        try
        {
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
            await _logService.LogAsync("User logged in.", user.Id, Repository.Enums.Log.LogEnum.Login.ToString(), string.Empty, dto.Email);
            return Ok(_response);
        }
        catch (System.Exception ex)
        {
            await _logService.LogAsync("Login failed.", 0, Repository.Enums.Log.LogEnum.Exception.ToString(), ex.StackTrace, dto.Email);
            throw;
        }

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
        try
        {
            var (message, id) = await _autService.RegisterAsync(dto);
            if (message == "Email already registered.")
            {
                _response.IsSuccess = false;
                _response.ErrorMessage = new List<string> { message }; ;
                return BadRequest(_response);
            }
            else if (message == "Username already exist.")
            {
                _response.IsSuccess = false;
                _response.ErrorMessage = new List<string> { "Username is not available." };
                return BadRequest(_response);
            }
            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Result = "Registered Successfully";
            await _logService.LogAsync("User registered.", id, Repository.Enums.Log.LogEnum.Register.ToString(), string.Empty, dto.Email);
            return Ok(_response);
        }
        catch (System.Exception ex)
        {
            await _logService.LogAsync("Register failed.", 0, Repository.Enums.Log.LogEnum.Exception.ToString(), ex.StackTrace, dto.Email);
            throw;
        }
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<APIResponse>> Logout()
    {
        string userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
        Response.Cookies.Delete("AuthToken");
        _response.IsSuccess = true;
        _response.Result = "Logged out successfully.";
        await _logService.LogAsync("User logged out.", int.Parse(userId), Repository.Enums.Log.LogEnum.Logout.ToString(), string.Empty, string.Empty);
        return Ok(_response);
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpGet("reset-password")]
    public async Task<ActionResult<APIResponse>> ResetPassword(string token, string type)
    {
        string? email = await _autService.ValidateResetToken(token, type);
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

        var email = await _autService.ValidateResetToken(dto.Token, dto.Type);
        if (string.IsNullOrEmpty(email))
        {
            _response.IsSuccess = false;
            _response.ErrorMessage = new List<string> { "Your token is expired" };
            return BadRequest(_response);
        }

        try
        {
            var result = await _autService.ResetPasswordAsync(email, dto);
            if (result == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessage = new List<string> { "Failed to reset password" };
                await _logService.LogAsync("Setup password failed.", 0, Repository.Enums.Log.LogEnum.Exception.ToString(),"", JsonSerializer.Serialize(dto));
                return BadRequest(_response);
            }

            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = "Password reset successfully";
            await _logService.LogAsync("Setup password successfully.", result.Id, Repository.Enums.Log.LogEnum.Update.ToString(), string.Empty, JsonSerializer.Serialize(dto));
            return Ok(_response);
        }
        catch (System.Exception ex)
        {
            await _logService.LogAsync("Setup password failed.", 0, Repository.Enums.Log.LogEnum.Exception.ToString(),ex.StackTrace, JsonSerializer.Serialize(dto));
            throw;
        }
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<APIResponse>> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        if (!ModelState.IsValid)
        {
            _response.IsSuccess = false;
            _response.ErrorMessage = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(_response);
        }

        try
        {
            var (message, id) = await _autService.ForgotPassword(dto.Email);
            if (message == "User not Exist.")
            {
                _response.IsSuccess = false;
                _response.ErrorMessage = new List<string> { message };
                return BadRequest(_response);
            }

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Result = "Mail sent successfully";
            await _logService.LogAsync("Forgot password.", id, Repository.Enums.Log.LogEnum.Update.ToString(), string.Empty, dto.Email);
            return Ok(_response);
        }
        catch (System.Exception ex)
        {
            await _logService.LogAsync("Forgot password failed.", 0, Repository.Enums.Log.LogEnum.Exception.ToString(), ex.StackTrace, dto.Email);
            throw;
        }
    }


}
