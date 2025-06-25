using TMS.Repository.Data;
using TMS.Repository.Dtos;

namespace TMS.Service.Interfaces;

public interface INotificationService
{
    public Task<Notification> AddNotification(int userId, int taskId, int status);
    public Task<List<NotificationDto>> GetNotificationAsync(int userId);
    public Task<string> MarkAsRead(int id);
    public Task<string> MarkAllAsRead(int userId);
}
