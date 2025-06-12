using System;
using System.Collections.Generic;

namespace TMS.Repository.Data;

public partial class Task
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<SubTask> SubTasks { get; set; } = new List<SubTask>();

    public virtual ICollection<TaskAssign> TaskAssigns { get; set; } = new List<TaskAssign>();
}
