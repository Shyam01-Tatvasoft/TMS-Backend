using Microsoft.EntityFrameworkCore;
using TMS.Repository.Data;
using TMS.Repository.Interfaces;

namespace TMS.Repository.Implementations;

public class NotificationRepository : INotificationRepository
{
    private readonly TmsContext _context;
    
    public NotificationRepository(TmsContext context)
    {
        _context = context;
    }

    public async Task<List<Notification>> GetNotificationsAsync(int userId)
    {
        return await _context.Notifications
            .Where(n => n.FkUserId == userId && n.IsRead == false)
            .Include(n => n.FkTask)
            .ThenInclude(n => n.FkTask)
            .ToListAsync();
    }

    public async Task<bool> AddNotification(Notification notification)
    {
        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> UpdateNotificationAsync(int id)
    {
        Notification? notification = await _context.Notifications.Where(n => n.Id == id && n.IsRead == false).FirstOrDefaultAsync();
        if(notification?.IsRead == true)
        {
            return 0;
        }
        if(notification != null)
        {
            notification.IsRead = true;
            await _context.SaveChangesAsync();
            return 1;
        }else{
            return -1;
        }
    }
}
