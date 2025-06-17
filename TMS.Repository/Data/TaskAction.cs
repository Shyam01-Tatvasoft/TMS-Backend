using System;
using System.Collections.Generic;

namespace TMS.Repository.Data;

public partial class TaskAction
{
    public int Id { get; set; }

    public int? FkTaskId { get; set; }

    public int? FkUserId { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public string? SubmittedData { get; set; }

    public virtual TaskAssign? FkTask { get; set; }

    public virtual User? FkUser { get; set; }
}
