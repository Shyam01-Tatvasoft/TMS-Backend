using TMS.Repository.Data;
using TMS.Repository.Dtos;
using TMS.Repository.Enums;
using TMS.Repository.Interfaces;
using TMS.Service.Interfaces;

namespace TMS.Service.Implementations;

public class NotificationService : INotificationService
{

    private readonly INotificationRepository _notificationRepository;

    public NotificationService(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<List<NotificationDto>> GetNotificationAsync(int userId)
    {
        List<Notification> notifications = await _notificationRepository.GetNotificationsAsync(userId);
        List<NotificationDto> notificationDtos = new();
        notifications.ForEach(n =>
        {
            notificationDtos.Add(new NotificationDto
            {
                Id = n.Id,
                FkTaskId = n.FkTaskId,
                FkUserId = n.FkUserId,
                TaskType = n.FkTask.FkTask.Name,
                TaskDescription = n.FkTask.Description,
                Priority = n.FkTask.Priority.HasValue ? ((Priority.PriorityEnum)n.FkTask.Priority.Value).ToString() : "Unknown",
                Status = n.FkTask.Status.HasValue ? ((Status.StatusEnum)n.FkTask.Status.Value).ToDescription() : "Unknown",
                IsRead = n.IsRead,
                UserName = n.FkTask.FkUser != null ? n.FkTask.FkUser.FirstName + " " + n.FkTask.FkUser.LastName : string.Empty
            });
        });

        return notificationDtos;
    }

    public async Task<Notification> AddNotification(int userId, int taskId)
    {
        Notification notification = new()
        {
            FkUserId = userId,
            FkTaskId = taskId,
            IsRead = false
        };
        await _notificationRepository.AddNotification(notification);
        return notification;
    }

    public async Task<string> MarkAsRead(int id)
    {
        int result =  await _notificationRepository.UpdateNotificationAsync(id);
        if(result == 1)
        {
            return "Notification mark as read.";
        }else if(result == 0)
        {
            return "Notification is already read";
        }else{
            return "Notification not found.";
        }
    }
}
