using System;
using System.Collections.Generic;

namespace TMS.Repository.Data;

public partial class SubTask
{
    public int Id { get; set; }

    public int? FkTaskId { get; set; }

    public string Name { get; set; } = null!;

    public virtual Task? FkTask { get; set; }

    public virtual ICollection<TaskAssign> TaskAssigns { get; set; } = new List<TaskAssign>();
}
