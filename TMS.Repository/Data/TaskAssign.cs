using System;
using System.Collections.Generic;

namespace TMS.Repository.Data;

public partial class TaskAssign
{
    public int Id { get; set; }

    public string? Description { get; set; }

    public int? FkUserId { get; set; }

    public int? FkTaskId { get; set; }

    public string? TaskData { get; set; }

    public DateTime DueDate { get; set; }

    public int? Status { get; set; }

    public int? Priority { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? FkSubtaskId { get; set; }

    public virtual SubTask? FkSubtask { get; set; }

    public virtual Task? FkTask { get; set; }

    public virtual User? FkUser { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<TaskAction> TaskActions { get; set; } = new List<TaskAction>();
}
