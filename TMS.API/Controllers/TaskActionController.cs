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
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", "1", "User Performed an action.");
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
            int? taskAction = await _taskActionService.AddUploadTaskAsync(dto);
            if (taskAction == null)
            {
                return StatusCode(500, "Failed to add task action.");
            }
            await _hubContext.Clients.All.SendAsync("ReceiveNotification","1", "User Performed an action.");
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
    public async Task<FileContentResult> DownloadDecryptedFile(string filename, int userId)
    {
        var user = await _userService.GetUserById(userId);
        if (user == null)
            throw new Exception("User not found");

        string combinedUserInfo = 
            user?.FirstName.Substring(0, 2) +
            user?.LastName.Substring(user.LastName.Length - 2) +
            user?.Phone?.Substring(0, 1) +
            user?.Email.Substring(0, 3);
        byte[] key = SHA256.HashData(Encoding.UTF8.GetBytes(combinedUserInfo)); // 32 bytes
        byte[] iv = MD5.HashData(Encoding.UTF8.GetBytes(combinedUserInfo));     // 16 bytes

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedEncrypted", filename);
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

        return new FileContentResult(fileBytes, "application/octet-stream")
        {
            FileDownloadName = "Decrypted_" + Path.GetFileNameWithoutExtension(filename)
        };
    }


    [HttpGet("download-zip")]
    [ProducesResponseType(typeof(FileResult), 200)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> DownloadFilesAsZip(int id, string userId)
    {
        var submittedData = await _taskActionService.GetTaskFileData(id);
        if (submittedData == null || !submittedData.Any())
            return NotFound("No file found");

        // Get User
        var user = await _userService.GetUserById(int.Parse(userId));
        if (user == null)
            return BadRequest("Invalid user");

        // Generate Key and IV from user details
        string combinedUserInfo = 
            user?.FirstName.Substring(0, 2) +
            user?.LastName.Substring(user.LastName.Length - 2) +
            user?.Phone?.Substring(0, 1) +
            user?.Email.Substring(0, 3);
        byte[] key = SHA256.HashData(Encoding.UTF8.GetBytes(combinedUserInfo));
        byte[] iv = MD5.HashData(Encoding.UTF8.GetBytes(combinedUserInfo));

        // Prepare temp directory for decrypted files
        string tempDir = Path.Combine(Path.GetTempPath(), $"Task_{id}_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedEncrypted");

        foreach (var file in submittedData)
        {
            string sourcePath = Path.Combine(uploadPath, file.StoredFileName);
            string destPath = Path.Combine(tempDir, file.OriginalFileName);

            using var inputStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read);
            using var outputStream = new FileStream(destPath, FileMode.Create);
            using var aes = Aes.Create();

            aes.Key = key;
            aes.IV = iv;

            using var cryptoStream = new CryptoStream(inputStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
            await cryptoStream.CopyToAsync(outputStream);
        }

        // Create zip archive
        string zipPath = Path.Combine(Path.GetTempPath(), $"Task_{id}_Files.zip");
        if (System.IO.File.Exists(zipPath)) System.IO.File.Delete(zipPath);

        ZipFile.CreateFromDirectory(tempDir, zipPath);

        // Clean up extracted files directory (optional)
        Directory.Delete(tempDir, true);

        var zipBytes = await System.IO.File.ReadAllBytesAsync(zipPath);
        System.IO.File.Delete(zipPath); 

        return File(zipBytes, "application/zip", $"TaskAction_{id}_Files.zip");
    }

}
