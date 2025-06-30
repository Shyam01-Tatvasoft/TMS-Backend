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
    public async Task<IActionResult> GetCountries()
    {
        // string? userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        try
        {
            List<CountryDto> result = await _countryService.GetCountries();
            // await _logService.LogAsync("Get all countries.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Read.ToString(), string.Empty, string.Empty);
            return Ok(result);
        }
        catch (System.Exception ex)
        {
            // await _logService.LogAsync("Get all countries.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Exception.ToString(), ex.StackTrace, string.Empty);
            return StatusCode(500, "Error retrieving countries: " + ex.Message);
        }
    }

    [HttpGet("timezone/{id:int}")]
    public async Task<IActionResult> GetTimezone(int id)
    {
        // string? userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        try
        {
            List<CountryTimezoneDto> timezones = await _countryService.GetTimezonesByCountryId(id);
            if (timezones == null)
            {
                return BadRequest("Invalid country ID or no timezones found for this country.");
            }
           
            // await _logService.LogAsync("Get timezones.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Read.ToString(), string.Empty, id.ToString());
            return Ok(timezones);
        }
        catch (System.Exception ex)
        {
            // await _logService.LogAsync("Get timezones.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Exception.ToString(), ex.StackTrace, id.ToString());
            return StatusCode(500, "Error retrieving timezones: " + ex.Message);
        }
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPost("import")]
    public async Task<IActionResult> ImportCountries()
    {
        string result = await _countryService.ImportCountriesAsync();
        return Ok(new { message = result });
    }
}
