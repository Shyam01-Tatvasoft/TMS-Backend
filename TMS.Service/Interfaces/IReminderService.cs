namespace TMS.Service.Interfaces;

public interface IReminderService
{
    public Task SendTaskNotification(string userId, string message);
}
