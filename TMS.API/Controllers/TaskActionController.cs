using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TMS.API.Hubs;
using TMS.Repository.Dtos;
using TMS.Service.Interfaces;

namespace TMS.API.Controllers;

[Route("api/task-action")]
[ApiController]
[EnableCors("AllowSpecificOrigin")]
public class TaskActionController : ControllerBase
{
    private readonly ITaskActionService _taskActionService;
    private readonly IJWTService _jwtService;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IUserService _userService;

    public TaskActionController(ITaskActionService taskActionService, IJWTService jwtService, IHubContext<NotificationHub> hubContext, IUserService userService)
    {
        _jwtService = jwtService;
        _taskActionService = taskActionService;
        _hubContext = hubContext;
        _userService = userService;
    }

    [HttpPost("send-email")]
    [ProducesResponseType(typeof(TaskActionDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> SendEmail([FromBody] EmailTaskDto dto)
    {
        try
        {
            if (dto == null || string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Subject))
            {
                return BadRequest("Invalid email task data.");
            }

            var taskAction = await _taskActionService.AddTaskActionAsync(dto);
            if (taskAction == null)
            {
                return StatusCode(500, "Failed to add task action.");
            }
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", 1, "User Performed an action.");
            return Ok(taskAction);
        }
        catch (System.Exception)
        {
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpPost("upload-file")]
    [ProducesResponseType(typeof(int), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> UploadFiles([FromForm] UploadFileTaskDto dto)
    {
        var files = dto.Files;

        if (files.Count == 0)
            return BadRequest("No files received.");

        try
        {
            int taskAction = await _taskActionService.AddUploadTaskAsync(dto);
            if (taskAction == null)
            {
                return StatusCode(500, "Failed to add task action.");
            }
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", 1, "User Performed an action.");
            return Ok(taskAction);
        }
        catch (System.Exception)
        {
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TaskActionDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetTaskActionById(int id)
    {
        var authToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        if (string.IsNullOrEmpty(authToken))
        {
            return Unauthorized();
        }
        try
        {
            var (email, role, userId) = _jwtService.ValidateToken(authToken);
            if (email == null || role == null || userId == null)
            {
                return Unauthorized();
            }
            TaskActionDto? taskAction = await _taskActionService.GetTaskActionByIdAsync(id);
            if (taskAction == null)
            {
                return NotFound("Task action not found.");
            }
            if (role == "User" && taskAction.FkUserId != null && taskAction.FkUserId != int.Parse(userId))
            {
                return Unauthorized("You do not have permission to access this task action.");
            }
            return Ok(taskAction);
        }
        catch (System.Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("download")]
    [ProducesResponseType(typeof(FileResult), 200)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult DownloadDecryptedFile(string fileName)
    {
        var encryptedPath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedEncrypted", fileName);

        if (!System.IO.File.Exists(encryptedPath))
            return NotFound("File not found.");

        byte[] decryptedBytes;

        using (var ms = new MemoryStream())
        {
            using var aes = Aes.Create();
            // UserDto? user = await _userService.GetUserById(int.Parse(userId));
            // string secret_iv = user.FirstName.Substring(0, 1) + user.FirstName.Substring(user.FirstName.Length - 2) + user.LastName.Substring(0, 1) + user.Phone;
            // string secret_key = secret_iv + secret_iv;
            
            // aes.Key = Encoding.UTF8.GetBytes(secret_key); 
            // aes.IV = Encoding.UTF8.GetBytes(secret_iv); 
            aes.Key = Encoding.UTF8.GetBytes("12345678901234567890123456789012");
            aes.IV = Encoding.UTF8.GetBytes("1234567890123456");

            using var decryptor = aes.CreateDecryptor();
            using var cryptoStream = new CryptoStream(ms, decryptor, CryptoStreamMode.Write);
            using var fileStream = new FileStream(encryptedPath, FileMode.Open);
            fileStream.CopyTo(cryptoStream);
            cryptoStream.FlushFinalBlock();
            decryptedBytes = ms.ToArray();
        }

        return File(decryptedBytes, "application/octet-stream", Path.GetFileName(fileName));
    }


    [HttpGet("download-zip/{id}/{userId}")]
    public async Task<IActionResult> DownloadFilesAsZip(int id,string userId)
    {
        List<TaskFileData>? submittedData = await _taskActionService.GetTaskFileData(id);
        if (submittedData == null)
            return NotFound("No file Found");

        var tempDir = Path.Combine(Path.GetTempPath(), $"Task_{id}");
        if (Directory.Exists(tempDir))
            Directory.Delete(tempDir, true);
        Directory.CreateDirectory(tempDir);

        string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedEncrypted");
        
        // UserDto? user = await _userService.GetUserById(int.Parse(userId));
        // string secret_iv = user.FirstName.Substring(0, 1) + user.FirstName.Substring(user.FirstName.Length - 2) + user.LastName.Substring(0, 1) + user.Phone;
        // string secret_key = secret_iv + secret_iv;
        
        foreach (var file in submittedData!)
        {
            string sourcePath = Path.Combine(uploadPath, file.StoredFileName);
            string destPath = Path.Combine(tempDir, file.OriginalFileName);

            using var inputStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read);
            using var outputStream = new FileStream(destPath, FileMode.Create);
            using var aes = Aes.Create();

            aes.Key = Encoding.UTF8.GetBytes("12345678901234567890123456789012");
            aes.IV = Encoding.UTF8.GetBytes("1234567890123456");

            using var decryptor = aes.CreateDecryptor();
            using var cryptoStream = new CryptoStream(inputStream, decryptor, CryptoStreamMode.Read);
            await cryptoStream.CopyToAsync(outputStream);
        }

        var zipPath = Path.Combine(Path.GetTempPath(), $"Task_{id}_Files.zip");
        if (System.IO.File.Exists(zipPath)) System.IO.File.Delete(zipPath);
        ZipFile.CreateFromDirectory(tempDir, zipPath);

        var zipBytes = await System.IO.File.ReadAllBytesAsync(zipPath);
        return File(zipBytes, "application/zip", $"TaskAction_{id}_Files.zip");
    }

}
