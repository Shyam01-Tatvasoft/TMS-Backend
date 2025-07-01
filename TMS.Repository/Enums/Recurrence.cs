using System.ComponentModel;

namespace TMS.Repository.Enums;

public class Recurrence
{
    public enum RecurrenceEnum
    {
        [Description("Daily")]
        Daily = 1,
        [Description("Weekly")]
        Weekly = 2,
        [Description("Monthly")]
        Monthly = 3,
    }
}
