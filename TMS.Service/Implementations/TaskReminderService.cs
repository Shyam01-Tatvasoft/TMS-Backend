using Microsoft.AspNetCore.SignalR;
using TMS.Repository.Data;
using TMS.Repository.Enums;
using TMS.Repository.Interfaces;
using TMS.Service.Interfaces;

namespace TMS.Service.Implementations;

public class TaskReminderService : ITaskReminderService
{
    private readonly ITaskAssignRepository _taskAssignRepository;
    private readonly IHubContext<ReminderHub> _hubContext;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly ILogService _logService;
    public TaskReminderService(ITaskAssignRepository taskAssignRepository, IHubContext<ReminderHub> hubContext, INotificationService notificationService, ILogService logService, IEmailService emailService)
    {
        _taskAssignRepository = taskAssignRepository;
        _hubContext = hubContext;
        _notificationService = notificationService;
        _logService = logService;
        _emailService = emailService;
    }

    public async System.Threading.Tasks.Task DueDateReminderService()
    {
        List<TaskAssign> tasksDue = await _taskAssignRepository.GetDueTasksAsync();
        foreach (var task in tasksDue)
        {
            var userId = task.FkUser?.Id.ToString();
            if (userId != null)
            {
                string message = $"Reminder: Your task is due tomorrow!";
                await _notificationService.AddNotification(task.FkUser.Id, task.Id, (int)Repository.Enums.Notification.NotificationEnum.Reminder);

                await _hubContext.Clients.All.SendAsync("ReceiveNotification", task.FkUserId, message);
                string emailBodyAdmin = await GetTaskEmailBody(task.Id, "ReminderMail");
                await _emailService.SendMail(task?.FkUser?.Email!, "Task DueDate Reminder", emailBodyAdmin);
                
            }
        }
    }

    public async System.Threading.Tasks.Task OverdueReminderService()
    {
        List<TaskAssign> overdueTasks = await _taskAssignRepository.GetOverdueTasksAsync();
        foreach (var task in overdueTasks)
        {
            var userId = task.FkUser?.Id.ToString();
            if (userId != null)
            {
                string message = $"Reminder: Your task is overdue!";
                await _notificationService.AddNotification(task.FkUser.Id, task.Id, (int)Repository.Enums.Notification.NotificationEnum.Overdue);

                await _hubContext.Clients.All.SendAsync("ReceiveNotification", task.FkUserId, message);
                string emailBodyAdmin = await GetTaskEmailBody(task.Id, "OverdueMail");
                await _emailService.SendMail(task?.FkUser?.Email!, "Task Overdue Reminder", emailBodyAdmin);
            }
        }
    }

    public async System.Threading.Tasks.Task RecurrentTaskAssignmentService()
    {
        List<TaskAssign> overdueTasks = await _taskAssignRepository.GetTodaysRecurrentTasksAsync();
        foreach (var task in overdueTasks)
        {
            var userId = task.FkUser?.Id.ToString();
            if (userId != null)
            {
                string message = "Assigned recurrent task";
                await _notificationService.AddNotification(task.FkUser.Id, task.Id, (int)Repository.Enums.Notification.NotificationEnum.Recurrence);

                await _hubContext.Clients.All.SendAsync("ReceiveNotification", task.FkUserId, message);
                string emailBodyAdmin = await GetTaskEmailBody(task.Id, "TaskEmailTemplate");
                await _emailService.SendMail(task?.FkUser?.Email!, "New Task Assigned", emailBodyAdmin);
            }
        }
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
        emailBody = emailBody.Replace("{{Priority}}", ((Priority.PriorityEnum)task?.Priority.Value).ToString() ?? "-");
        emailBody = emailBody.Replace("{{Status}}", ((Status.StatusEnum)task?.Status.Value).ToString() ?? "-");
        emailBody = emailBody.Replace("{{DueDate}}", task.DueDate.ToString("dd MMM yyyy"));
        emailBody = emailBody.Replace("{{Description}}", task.Description ?? "-");

        return emailBody;
    }
}
