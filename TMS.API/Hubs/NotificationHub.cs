using Microsoft.AspNetCore.SignalR;

namespace TMS.API.Hubs;

public class NotificationHub : Hub
{
    public async Task SendTaskNotification(string userId, string message)
    {
        await Clients.All.SendAsync("ReceiveNotification", userId, message);
    }
}
