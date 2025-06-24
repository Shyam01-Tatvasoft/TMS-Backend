using System;
using System.Collections.Generic;

namespace TMS.Repository.Data;

public partial class Log
{
    public int Id { get; set; }

    public int? FkUserId { get; set; }

    public DateTime? Date { get; set; }

    public string? Action { get; set; }

    public string? Data { get; set; }

    public string? Message { get; set; }

    public string? Stacktrash { get; set; }

    public virtual User? FkUser { get; set; }
}
