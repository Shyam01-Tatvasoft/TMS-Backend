using System.Net;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using TMS.Repository.Data;
using TMS.Repository.Dtos;
using TMS.Repository.Interfaces;
using TMS.Service.Interfaces;

namespace TMS.API.Controllers;


[Route("api/[controller]")]
[EnableCors("AllowSpecificOrigin")]
[ApiController]
public class CountryController : ControllerBase
{
    private readonly ICountryService _countryService;
    private readonly APIResponse _response;
    public CountryController(ICountryService countryService)
    {
       _countryService = countryService;
       this._response = new APIResponse();
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
            List<TimezoneDetail> timezones = await _countryService.GetTimezonesByCountryId(id);
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

    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPost("import")]
    public async Task<IActionResult> ImportCountries()
    {
        var result = await _countryService.ImportCountriesAsync();
        return Ok(new { message = result });
    }
}
