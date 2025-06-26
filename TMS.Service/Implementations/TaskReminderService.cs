using Microsoft.AspNetCore.SignalR;
using TMS.Repository.Data;
using TMS.Repository.Interfaces;
using TMS.Service.Interfaces;

namespace TMS.Service.Implementations;

public class TaskReminderService : ITaskReminderService
{
    private readonly ITaskAssignRepository _taskAssignRepository;
    private readonly IHubContext<ReminderService> _hubContext;
    private readonly INotificationService _notificationService;
    private readonly ILogService _logService;
    public TaskReminderService(ITaskAssignRepository taskAssignRepository, IHubContext<ReminderService> hubContext, INotificationService notificationService, ILogService logService)
    {
        _taskAssignRepository = taskAssignRepository;
        _hubContext = hubContext;
        _notificationService = notificationService;
        _logService = logService;
    }

    public async System.Threading.Tasks.Task DueDateReminderService()
    {
        List<TaskAssign> tasksDue = await _taskAssignRepository.GetDueTasksAsync();
        foreach (var task in tasksDue)
        {
            var userId = task.FkUser?.Id.ToString();
            if (userId != null)
            {
                string message = $"Reminder: Your task (ID: {task.Id}) is due tomorrow!";
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", task.FkUserId, message);

                
                await _notificationService.AddNotification(task.FkUser.Id, task.Id, (int)Repository.Enums.Notification.NotificationEnum.Reminder);
            }
        }
    }
}
