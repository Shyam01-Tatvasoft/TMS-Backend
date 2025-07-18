using System.Text.Json;
using iText.Commons.Utils;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using TMS.Repository.Data;
using TMS.Repository.Dtos;
using TMS.Repository.Enums;
using TMS.Service.Interfaces;

namespace TMS.API.Controllers;

[Route("api/email")]
[EnableCors("AllowSpecificOrigin")]
[ApiController]
public class EmailController : ControllerBase
{

    private readonly IEmailService _emailService;
    private readonly ILogService _logService;
    private readonly IJWTService _jwtService;
    public EmailController(IEmailService emailService, ILogService logService, IJWTService jwtService)
    {
        _emailService = emailService;
        _logService = logService;
        _jwtService = jwtService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<EmailTemplateDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetEmailTemplates()
    {
        string authToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        if (string.IsNullOrEmpty(authToken))
            return Unauthorized();
        var (email, role, userId) = _jwtService.ValidateToken(authToken);
        if (role != "Admin")
            return Forbid("You do not have permission to access this resource.");
        try
        {
            List<EmailTemplateDto> emailTemplates = await _emailService.GetEmailTemplates();
            if (emailTemplates == null || !emailTemplates.Any())
            {
                return NotFound("No email templates found.");
            }
            await _logService.LogAsync("Email templates retrieved successfully.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Read.ToString(), String.Empty, string.Empty);
            return Ok(emailTemplates);
        }
        catch (System.Exception ex)
        {
            await _logService.LogAsync("Error retrieving email templates.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Exception.ToString(), ex.StackTrace, string.Empty);
            return StatusCode(500, "Internal server error while retrieving email templates.");
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> AddEmailTemplate([FromBody] EmailTemplateDto template)
    {
            string authToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (string.IsNullOrEmpty(authToken))
                return Unauthorized();
            var (email, role, userId) = _jwtService.ValidateToken(authToken);
        try
        {
            if (role != "Admin")
                return Forbid("You do not have permission to access this resource.");

            if (template == null || string.IsNullOrEmpty(template.Name) || string.IsNullOrEmpty(template.Body))
            {
                return BadRequest("Invalid email template data.");
            }

            var (isAdded, message) = await _emailService.AddEmailTemplate(template);
            if (!isAdded)
            {
                return BadRequest(message);
            }

            await _logService.LogAsync($"Email template '{template.Name}' added successfully.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Create.ToString(), String.Empty, string.Empty);
            return Ok(message);
        }
        catch (System.Exception)
        {
            await _logService.LogAsync("Error adding email template.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Exception.ToString(), String.Empty, string.Empty);
            return StatusCode(500, "Internal server error while adding email template.");
        }
    }

    [HttpGet("template/{Id}")]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetEmailTemplateByName(int id)
    {
        string authToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        if (string.IsNullOrEmpty(authToken))
            return Unauthorized();
        var (email, role, userId) = _jwtService.ValidateToken(authToken);
        if (role != "Admin")
            return Forbid("You do not have permission to access this resource.");
        try
        {
            EmailTemplateDto? template = await _emailService.GetEmailTemplateById(id);
            if (template == null)
            {
                return NotFound($"Email template with ID {id} not found.");
            }
            await _logService.LogAsync($"Email template retrieved successfully.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Read.ToString(), String.Empty, id.ToString());
            return Ok(template);
        }
        catch (System.Exception ex)
        {
            await _logService.LogAsync($"Error retrieving email template.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Exception.ToString(), ex.StackTrace, id.ToString());
            return StatusCode(500, $"Internal server error while retrieving email template");
        }
    }

    [HttpPut]
    [ProducesResponseType(typeof(EmailTemplateDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> UpdateEmailTemplate([FromBody] EmailTemplateDto template)
    {
        string authToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        if (string.IsNullOrEmpty(authToken))
            return Unauthorized();
        var (email, role, userId) = _jwtService.ValidateToken(authToken);
        if (role != "Admin")
            return Forbid("You do not have permission to access this resource.");
        try
        {
            if (template == null || string.IsNullOrEmpty(template.Name) || string.IsNullOrEmpty(template.Body))
            {
                return BadRequest("Invalid email template data.");
            }

            EmailTemplate? updatedTemplate = await _emailService.UpdateAsync(template);
            if (updatedTemplate == null)
            {
                return NotFound($"Email template not found.");
            }
            await _logService.LogAsync($"Email template updated successfully.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Update.ToString(), String.Empty, JsonSerializer.Serialize(template));
            return Ok(updatedTemplate);
        }
        catch (System.Exception ex)
        {
            await _logService.LogAsync($"Error updating email template.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Exception.ToString(), ex.StackTrace, JsonSerializer.Serialize(template));
            return StatusCode(500, $"Internal server error while updating email template.");
        }
    }
}
