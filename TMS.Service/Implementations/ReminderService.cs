using Microsoft.AspNetCore.SignalR;

namespace TMS.Service.Implementations;

public class ReminderService : Hub 
{
    public async Task SendTaskNotification(string userId, string message)
    {
        await Clients.All.SendAsync("ReceiveReminder", userId, message);
    }
}
