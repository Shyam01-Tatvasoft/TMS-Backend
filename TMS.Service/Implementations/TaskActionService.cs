using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TMS.Repository.Data;
using TMS.Repository.Dtos;
using TMS.Repository.Enums;
using TMS.Repository.Interfaces;
using TMS.Service.Interfaces;

namespace TMS.Service.Implementations;

public class TaskActionService : ITaskActionService
{
    private readonly ITaskActionRepository _taskActionRepository;
    private readonly IEmailService _emailService;
    private readonly ITaskAssignRepository _taskAssignRepository;

    public TaskActionService(ITaskActionRepository taskActionRepository, IEmailService emailService, ITaskAssignRepository taskAssignRepository)
    {
        _taskActionRepository = taskActionRepository;
        _emailService = emailService;
        _taskAssignRepository = taskAssignRepository;
    }

    public async Task<List<TaskAction>> GetAllTaskActionsAsync()
    {
        return await _taskActionRepository.GetAllTaskActionsAsync();
    }

    public async Task<TaskAction?> GetTaskActionByIdAsync(int id)
    {
        return await _taskActionRepository.GetTaskActionByIdAsync(id);
    }

    public async Task<int> AddTaskActionAsync(EmailTaskDto emailTask)
    {
        var emailInfo = new
        {
            emailTask.Email,
            emailTask.Subject,
            DateTime.Now
        };

        var taskAction = new TaskAction
        {
            FkTaskId = emailTask.FkTaskId,
            FkUserId = emailTask.FkUserId,
            SubmittedAt = DateTime.Now,
            SubmittedData = JsonSerializer.Serialize(emailInfo)
        };
        
        string emailBody = await GetTaskEmailBody("WelcomeMailTemplate");
        _emailService.SendMail(emailTask.Email, emailTask.Subject, emailBody);
        await _taskActionRepository.AddTaskActionAsync(taskAction);

        TaskAssign? task = await _taskAssignRepository.GetTaskAssignAsync(emailTask.FkTaskId);
        task.Status = (int?)Status.StatusEnum.Review;

        await _taskAssignRepository.UpdateTaskAssignAsync(task);
        return taskAction.Id;
    }

    public async Task<int> AddUploadTaskAsync(UploadFileTaskDto dto)
    {
        
        var files = dto.Files;
        var taskId = dto.FkTaskId;
        var userId = dto.FkUserId;

        var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "UploadedEncrypted");
        if (!Directory.Exists(uploadFolder))
            Directory.CreateDirectory(uploadFolder);

        var submittedDataList = new List<object>();

        foreach (var file in files)
        {
            var originalFileName = file.FileName;
            var encryptedFileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var encryptedPath = Path.Combine(uploadFolder, encryptedFileName);

            // Encrypt file
            using (var fs = new FileStream(encryptedPath, FileMode.Create))
            using (var aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes("12345678901234567890123456789012"); // 32 bytes
                aes.IV = Encoding.UTF8.GetBytes("1234567890123456"); // 16 bytes

                using var cryptoStream = new CryptoStream(fs, aes.CreateEncryptor(), CryptoStreamMode.Write);
                await file.CopyToAsync(cryptoStream);
            }

            submittedDataList.Add(new
            {
                OriginalFileName = originalFileName,
                StoredFileName = encryptedFileName,
                Size = file.Length,
                UploadedAt = DateTime.Now
            });
        }

        var taskAction = new TaskAction
        {
            FkTaskId = taskId,
            FkUserId = userId,
            SubmittedAt = DateTime.Now,
            SubmittedData = JsonSerializer.Serialize(submittedDataList)
        };

        await _taskActionRepository.AddTaskActionAsync(taskAction);

        TaskAssign? task = await _taskAssignRepository.GetTaskAssignAsync(dto.FkTaskId);
        task.Status = (int?)Status.StatusEnum.Review;
        await _taskAssignRepository.UpdateTaskAssignAsync(task);
        
        return taskAction.Id;
        
    }

    public async Task<string> GetTaskEmailBody(string templateName = "WelcomeMailTemplate")
    {
        string templatePath = $"d:/TMS/TMS.Service/Templates/{templateName}.html";

        if (!System.IO.File.Exists(templatePath))
        {
            return "<p>Email template not found</p>";
        }

        string emailBody = System.IO.File.ReadAllText(templatePath);

        return emailBody;
    }
}
