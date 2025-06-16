using TMS.Repository.Data;

namespace TMS.Repository.Interfaces;

public interface INotificationRepository
{
    public Task<List<Notification>> GetNotificationsAsync(int userId);
    public Task<bool> AddNotification(Notification notification);
    public Task<int> UpdateNotificationAsync(int id);
}
