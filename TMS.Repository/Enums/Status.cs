using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TMS.Repository.Enums;

public class Status
{
    public enum StatusEnum
    {
        [Description("Pending")]
        Pending = 1,
        [Description("In Progress")]
        InProgress = 2,
        [Description("Completed")]
        Completed = 3,
        [Description("On Hold")]
        OnHold = 4,
        [Description("Cancelled")]
        Cancelled = 5,
        [Description("Review")]
        Review = 6
    }
}
