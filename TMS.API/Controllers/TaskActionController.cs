using System.IO.Compression;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
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
    private readonly ILogService _logService;

    public TaskActionController(ITaskActionService taskActionService, IJWTService jwtService, IHubContext<NotificationHub> hubContext, IUserService userService, ILogService logService)
    {
        _jwtService = jwtService;
        _taskActionService = taskActionService;
        _hubContext = hubContext;
        _userService = userService;
        _logService = logService;
    }

    [HttpPost("send-email")]
    [ProducesResponseType(typeof(TaskActionDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> SendEmail([FromBody] EmailTaskDto dto)
    {
        string? userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
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
            // await _hubContext.Clients.All.SendAsync("ReceiveNotification", "1", "User Performed an action.");
            await _logService.LogAsync("Perform email task.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Create.ToString(), string.Empty, JsonSerializer.Serialize(dto));
            return Ok(taskAction);
        }
        catch (System.Exception ex)
        {
            await _logService.LogAsync("Perform email task.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Exception.ToString(), ex.StackTrace, JsonSerializer.Serialize(dto));
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpPost("upload-file")]
    [ProducesResponseType(typeof(int), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> UploadFiles([FromForm] UploadFileTaskDto dto)
    {
        string? userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var files = dto.Files;

        if (files.Count == 0)
            return BadRequest("No files received.");

        try
        {
            int? taskAction = await _taskActionService.AddUploadTaskAsync(dto);
            if (taskAction == null)
            {
                return StatusCode(500, "Failed to add task action.");
            }
            // await _hubContext.Clients.All.SendAsync("ReceiveNotification", "1", "User Performed an action.");
            await _logService.LogAsync("Perform upload file task.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Create.ToString(), string.Empty, JsonSerializer.Serialize(dto));
            return Ok(taskAction);
        }
        catch (System.Exception)
        {
            await _logService.LogAsync("Perform upload file task.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Exception.ToString(), string.Empty, JsonSerializer.Serialize(dto));
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
        var (email, role, userId) = _jwtService.ValidateToken(authToken);
        try
        {
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
            await _logService.LogAsync("Get task action.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Read.ToString(), string.Empty, id.ToString());
            return Ok(taskAction);
        }
        catch (System.Exception ex)
        {
            await _logService.LogAsync("Get task action.", int.Parse(userId!), Repository.Enums.Log.LogEnum.Exception.ToString(), ex.StackTrace, id.ToString());
            return StatusCode(500);
        }
    }

    [HttpGet("download")]
    [ProducesResponseType(typeof(FileResult), 200)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<FileContentResult> DownloadDecryptedFile(string filename, int userId)
    {
        string? id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        try
        {
            var user = await _userService.GetUserById(userId);
            if (user == null)
                throw new Exception("User not found");

            string combinedUserInfo =
                user?.FirstName.Substring(0, 2) +
                user?.LastName.Substring(user.LastName.Length - 2) +
                user?.Phone?.Substring(0, 1) +
                user?.Email.Substring(0, 3);
            byte[] key = SHA256.HashData(Encoding.UTF8.GetBytes(combinedUserInfo));
            byte[] iv = MD5.HashData(Encoding.UTF8.GetBytes(combinedUserInfo));

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Upload", userId.ToString(), filename);
            if (!System.IO.File.Exists(filePath))
                throw new FileNotFoundException("Encrypted file not found");

            using var encryptedFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            using var cryptoStream = new CryptoStream(encryptedFileStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var memoryStream = new MemoryStream();
            await cryptoStream.CopyToAsync(memoryStream);

            byte[] fileBytes = memoryStream.ToArray();

            await _logService.LogAsync("Download file.", int.Parse(id!), Repository.Enums.Log.LogEnum.Read.ToString(), string.Empty, filename);
            return new FileContentResult(fileBytes, "application/octet-stream")
            {
                FileDownloadName = "Decrypted_" + Path.GetFileNameWithoutExtension(filename)
            };
        }
        catch (System.Exception)
        {
            await _logService.LogAsync("Download file.", int.Parse(id!), Repository.Enums.Log.LogEnum.Exception.ToString(), string.Empty, filename);
            throw;
        }

    }

    [HttpGet("download-zip")]
    [ProducesResponseType(typeof(FileResult), 200)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> DownloadFilesAsZip(int id, string userId)
    {
        string? authUserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        try
        {
            var submittedData = await _taskActionService.GetTaskFileData(id);
            if (submittedData == null || !submittedData.Any())
                return NotFound("No file found");

            var user = await _userService.GetUserById(int.Parse(userId));
            if (user == null)
                return BadRequest("Invalid user");

            string combinedUserInfo =
                user?.FirstName.Substring(0, 2) +
                user?.LastName.Substring(user.LastName.Length - 2) +
                user?.Phone?.Substring(0, 1) +
                user?.Email.Substring(0, 3);
            byte[] key = SHA256.HashData(Encoding.UTF8.GetBytes(combinedUserInfo));
            byte[] iv = MD5.HashData(Encoding.UTF8.GetBytes(combinedUserInfo));

            string tempDir = Path.Combine(Path.GetTempPath(), $"Task_{id}_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);

            string userUploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Upload", userId);

            foreach (var file in submittedData)
            {
                string sourcePath = Path.Combine(userUploadPath, file.StoredFileName);
                string destPath = Path.Combine(tempDir, file.OriginalFileName);

                using var inputStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read);
                using var outputStream = new FileStream(destPath, FileMode.Create);
                using var aes = Aes.Create();
                aes.Key = key;
                aes.IV = iv;

                using var cryptoStream = new CryptoStream(inputStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
                await cryptoStream.CopyToAsync(outputStream);
            }

            string zipPath = Path.Combine(Path.GetTempPath(), $"Task_{id}_Files.zip");
            if (System.IO.File.Exists(zipPath)) System.IO.File.Delete(zipPath);

            ZipFile.CreateFromDirectory(tempDir, zipPath);
            Directory.Delete(tempDir, true);

            var zipBytes = await System.IO.File.ReadAllBytesAsync(zipPath);
            System.IO.File.Delete(zipPath);
            
            await _logService.LogAsync("Download zip file.", int.Parse(authUserId!), Repository.Enums.Log.LogEnum.Read.ToString(), string.Empty, id.ToString());
            return File(zipBytes, "application/zip", $"TaskAction_{id}_Files.zip");
        }
        catch (System.Exception ex)
        {
            await _logService.LogAsync("Download zip file.", int.Parse(authUserId!), Repository.Enums.Log.LogEnum.Exception.ToString(), ex.StackTrace, id.ToString());
            throw;
        }
    }
}
