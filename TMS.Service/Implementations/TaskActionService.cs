using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Transactions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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
    private readonly INotificationService _notificationService;
    private readonly IUserService _userService;
    private readonly IHubContext<ReminderHub> _hubContext;
    public TaskActionService(ITaskActionRepository taskActionRepository, IEmailService emailService, ITaskAssignRepository taskAssignRepository, INotificationService notificationRepository, IUserService userService, IHubContext<ReminderHub> hubContext)
    {
        _taskActionRepository = taskActionRepository;
        _emailService = emailService;
        _taskAssignRepository = taskAssignRepository;
        _notificationService = notificationRepository;
        _userService = userService;
        _hubContext = hubContext;
    }

    public async Task<List<TaskAction>> GetAllTaskActionsAsync()
    {
        return await _taskActionRepository.GetAllTaskActionsAsync();
    }

    public async Task<TaskActionDto?> GetTaskActionByIdAsync(int id)
    {
        TaskAction? taskAction = await _taskActionRepository.GetTaskActionByIdAsync(id);
        if (taskAction == null)
            return null;
        TaskActionDto taskActionDto = new()
        {
            Id = taskAction.Id,
            FkTaskId = taskAction.FkTaskId,
            FkUserId = taskAction.FkUserId,
            SubmittedAt = taskAction.SubmittedAt,
            SubmittedData = JsonSerializer.Deserialize<JsonElement>(taskAction.SubmittedData!),
            UserName = taskAction.FkUser != null ? taskAction.FkUser.FirstName + " " + taskAction.FkUser.LastName : string.Empty,
            TaskName = taskAction.FkTask?.FkTask?.Name ?? string.Empty,
            SubTaskName = taskAction?.FkTask?.FkSubtask?.Name ?? string.Empty,
            Status = taskAction?.FkUser != null ? ((Status.StatusEnum)taskAction?.FkTask?.Status!).ToDescription() : string.Empty
        };

        return taskActionDto;
    }

    public async Task<int> AddEmailTaskAsync(EmailTaskDto emailTask)
    {
        using TransactionScope transaction = new(TransactionScopeAsyncFlowOption.Enabled);
        try
        {
            TaskAssign? taskAssigned = await _taskAssignRepository.GetTaskAssignAsync(emailTask.FkTaskId);
            if(taskAssigned != null && taskAssigned.DueDate < DateTime.Now)
            {
                return -1; // Task is overdue, cannot perform action
            }
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

            // perform email send task
            string emailBody = await GetTaskEmailBody("WelcomeMailTemplate");
            await _emailService.SendMail(emailTask.Email, emailTask.Subject, emailBody);
            if (emailTask.TaskActionId != null && emailTask.TaskActionId != 0)
            {
                // Perform update when Task is reassigned
                taskAction.Id = (int)emailTask.TaskActionId!;
                await _taskActionRepository.UpdateTaskActionAsync(taskAction);
            }
            else
            {
                await _taskActionRepository.AddTaskActionAsync(taskAction);
            }


            TaskAssign? task = await _taskAssignRepository.GetTaskAssignAsync(emailTask.FkTaskId);
            task.Status = (int?)Status.StatusEnum.Review;

            await _taskAssignRepository.UpdateTaskAssignAsync(task);

            // send mail to admin
            string emailBodyAdmin = await GetTaskEmailBody(emailTask.FkTaskId, "TaskPerformedTemplate");
            await _emailService.SendMail("admin1@gmail.com", "Task Performed", emailBodyAdmin);

            //send notification to admin
            await _notificationService.AddNotification(1, emailTask.FkTaskId, (int)Repository.Enums.Notification.NotificationEnum.Review);
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", "1", "User Performed an action.");
            transaction.Complete();
            return taskAction.Id;
        }
        catch (System.Exception)
        {
            transaction.Dispose();
            throw;
        }
    }

    public async Task<int> AddUploadTaskAsync(UploadFileTaskDto dto)
    {
        using TransactionScope transaction = new(TransactionScopeAsyncFlowOption.Enabled);
        try
        {
            TaskAssign? taskAssigned = await _taskAssignRepository.GetTaskAssignAsync(dto.FkTaskId);
            if(taskAssigned != null && taskAssigned.DueDate < DateTime.Now)
            {
                return -1; // Task is overdue, cannot perform action
            }
            var files = dto.Files;
            var taskId = dto.FkTaskId;
            var userId = dto.FkUserId;
            UserDto? user = await _userService.GetUserById(userId);
            var userFolder = Path.Combine(Directory.GetCurrentDirectory(), "Upload", userId.ToString());

            if (!Directory.Exists(userFolder))
                Directory.CreateDirectory(userFolder);

            // Generate Key and IV based on user details
            string combinedUserInfo =
                user?.FirstName[..2] +
                user?.LastName[(user.LastName.Length - 2)..] +
                user?.Phone?[..1] +
                user?.Email[..3];

            byte[] key = SHA256.HashData(Encoding.UTF8.GetBytes(combinedUserInfo)); // 32 bytes
            byte[] iv = MD5.HashData(Encoding.UTF8.GetBytes(combinedUserInfo));    // 16 bytes

            var submittedDataList = new List<object>();

            foreach (var file in files)
            {
                var originalFileName = file.FileName;
                var encryptedFileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                var encryptedPath = Path.Combine(userFolder, encryptedFileName);

                using (var fs = new FileStream(encryptedPath, FileMode.Create))
                using (var aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;

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

            TaskAction taskAction = new()
            {
                FkTaskId = taskId,
                FkUserId = userId,
                SubmittedAt = DateTime.Now,
                SubmittedData = JsonSerializer.Serialize(submittedDataList)
            };

            if (dto.TaskActionId != null && dto.TaskActionId != 0)
            {
                taskAction.Id = (int)dto.TaskActionId!;
                await _taskActionRepository.UpdateTaskActionAsync(taskAction);
            }
            else
            {
                await _taskActionRepository.AddTaskActionAsync(taskAction);
            }

            TaskAssign? task = await _taskAssignRepository.GetTaskAssignAsync(dto.FkTaskId);
            task.Status = (int?)Status.StatusEnum.Review;
            await _taskAssignRepository.UpdateTaskAssignAsync(task);

            // Send notifications
            string emailBodyAdmin = await GetTaskEmailBody(dto.FkTaskId, "TaskPerformedTemplate");
            await _emailService.SendMail("admin1@gmail.com", "Task Performed", emailBodyAdmin);
            await _notificationService.AddNotification(1, dto.FkTaskId, (int)Repository.Enums.Notification.NotificationEnum.Review);
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", "1", "User Performed an action.");
            transaction.Complete();
            return taskAction.Id;
        }
        catch (System.Exception)
        {
            transaction.Dispose();
            throw;
        }
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

    public async Task<string> GetTaskEmailBody(int id, string templateName)
    {
        TaskAssign? task = await _taskAssignRepository.GetTaskAssignAsync(id);
        string templatePath = $"d:/TMS/TMS.Service/Templates/{templateName}.html";

        if (!System.IO.File.Exists(templatePath))
        {
            return "<p>Email template not found</p>";
        }

        string emailBody = System.IO.File.ReadAllText(templatePath);

        emailBody = emailBody.Replace("{{UserName}}", task?.FkUser.FirstName + " " + task.FkUser.LastName ?? "User");
        emailBody = emailBody.Replace("{{TaskType}}", task.FkTask?.Name ?? "-");
        emailBody = emailBody.Replace("{{SubTask}}", task.FkSubtask?.Name ?? "-");
        emailBody = emailBody.Replace("{{Priority}}", ((Priority.PriorityEnum)task?.Priority.Value!).ToString() ?? "-");
        emailBody = emailBody.Replace("{{Status}}", ((Status.StatusEnum)task?.Status.Value!).ToString() ?? "-");
        emailBody = emailBody.Replace("{{DueDate}}", task.DueDate.ToString("dd MMM yyyy"));
        emailBody = emailBody.Replace("{{Description}}", task.Description ?? "-");

        return emailBody;
    }


    public async Task<List<TaskFileData>?> GetTaskFileData(int id)
    {
        TaskActionDto? taskAction = await GetTaskActionByIdAsync(id);
        if (taskAction == null)
            return null;

        var submittedData = JsonSerializer.Deserialize<List<TaskFileData>>((JsonElement)taskAction.SubmittedData!);
        return submittedData;
    }

    public async Task<RecurrenceTaskDto> GetRecurrentTaskDetail(string recurrenceId)
    {
        List<TaskAssign> taskAssigns =  await _taskAssignRepository.GetRecurrenceTaskAsync(recurrenceId);
        TaskAssign? firstTask = taskAssigns.First();
        taskAssigns = taskAssigns.OrderBy(t => t.DueDate).ToList();
        List<RecurrenceTaskActionDto> recurrenceTaskActions = new();
        foreach(var task in taskAssigns)
        {
            bool isReviewOrCompleted = task.Status == (int)Status.StatusEnum.Review || task.Status == (int)Status.StatusEnum.Completed;
            recurrenceTaskActions.Add(new RecurrenceTaskActionDto{
                TaskId = task.Id, 
                TaskActionId = isReviewOrCompleted ? task.TaskActions.First().Id : null,
                AssignedAt = task.CreatedAt,
                DueDate = task.DueDate,
                Status = ((Status.StatusEnum)task.Status!).ToDescription(),
                SubmittedAt = isReviewOrCompleted ? task.TaskActions.First().SubmittedAt : null,
                SubmittedData = isReviewOrCompleted ? JsonSerializer.Deserialize<JsonElement>(task.TaskActions.First().SubmittedData!) : null
            });
        }

        RecurrenceTaskDto recurrenceTaskDetail = new(){
            Id = recurrenceId,
            FkUserId = taskAssigns.First().FkUserId,
            TaskName = firstTask.FkTask?.Name,
            SubTaskName = firstTask.FkSubtask?.Name,
            UserName = firstTask.FkUser?.FirstName + " " + firstTask.FkUser?.LastName,
            TaskActionList = recurrenceTaskActions,
        };

        return recurrenceTaskDetail;
    }
}
