using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using TMS.Repository.Data;
using TMS.Repository.Dtos;
using TMS.Repository.Interfaces;
using TMS.Service.Interfaces;

namespace TMS.API.Controllers;


[Route("api/country")]
[EnableCors("AllowSpecificOrigin")]
[ApiController]
public class CountryController : ControllerBase
{
    private readonly ICountryService _countryService;
    private readonly APIResponse _response;
    private readonly ILogService _logService;
    public CountryController(ICountryService countryService, ILogService logService)
    {
       _countryService = countryService;
       _logService = logService;
       this._response = new APIResponse();
    }

     [HttpGet("countries")]
    public async Task<ActionResult<APIResponse>> GetCountries()
    {
        // string? userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        try
        {
            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Result = await _countryService.GetCountries();
            // await _logService.LogAsync("Get all countries.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Read.ToString(), string.Empty, string.Empty);
            return Ok(_response);
        }
        catch (System.Exception ex)
        {
            _response.IsSuccess = false;
            _response.StatusCode = HttpStatusCode.InternalServerError;
            _response.ErrorMessage = new List<string> { ex.Message };
            // await _logService.LogAsync("Get all countries.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Exception.ToString(), ex.StackTrace, string.Empty);
            return StatusCode((int)HttpStatusCode.InternalServerError, _response);
        }
    }

    [HttpGet("timezone/{id:int}")]
    public async Task<ActionResult<APIResponse>> GetTimezone(int id)
    {
        // string? userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        try
        {
            List<CountryTimezoneDto> timezones = await _countryService.GetTimezonesByCountryId(id);
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
            // await _logService.LogAsync("Get timezones.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Read.ToString(), string.Empty, id.ToString());
            return Ok(_response);
        }
        catch (System.Exception ex)
        {
            _response.IsSuccess = false;
            _response.StatusCode = HttpStatusCode.InternalServerError;
            _response.ErrorMessage = new List<string> { ex.Message };
            // await _logService.LogAsync("Get timezones.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Exception.ToString(), ex.StackTrace, id.ToString());
            return StatusCode((int)HttpStatusCode.InternalServerError, _response);
        }
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPost("import")]
    public async Task<IActionResult> ImportCountries()
    {
        var result = await _countryService.ImportCountriesAsync();
        return Ok(new { message = result });
    }
}
