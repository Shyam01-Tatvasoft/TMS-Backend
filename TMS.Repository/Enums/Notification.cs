using System.ComponentModel;

namespace TMS.Repository.Enums;

public class Notification
{
    public enum NotificationEnum
    {
        [Description("New Assigned")]
        Assigned = 1,
        [Description("Under Review")]
        Review = 2,
        [Description("Approved")]
        Approved = 3,
        [Description("Re Assigned")]
        Reassigned = 4,
        [Description("DueDate Reminder")]
        Reminder = 5,
        [Description("Overdue Reminder")]
        Overdue = 6,
    }
}
