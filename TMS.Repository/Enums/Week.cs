using System.ComponentModel;

namespace TMS.Repository.Enums;

public class Week
{
    public enum WeekEnum{
        [Description("Monday")]
        Monday = 1,
        [Description("Tuesday")]
        Tuesday = 2,
        [Description("Wednesday")]
        Wednesday = 3,
        [Description("Thursday")]
        Thursday = 4,
        [Description("Friday")]
        Friday = 5,
    }
}
