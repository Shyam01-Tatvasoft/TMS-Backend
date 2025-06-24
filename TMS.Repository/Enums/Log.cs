using System.ComponentModel;

namespace TMS.Repository.Enums;

public class Log
{
    public enum LogEnum{
        [Description("Exception")]
        Exception = 1,
        [Description("Create")]
        Create = 2,
        [Description("Update")]
        Update = 3,
        [Description("Delete")]
        Delete = 4,
        [Description("Read")]
        Read = 5,
        [Description("Error")]
        Error = 6
    }
}
