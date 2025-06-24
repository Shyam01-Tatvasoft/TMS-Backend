using System;
using System.Collections.Generic;

namespace TMS.Repository.Data;

public partial class Notification
{
    public int Id { get; set; }

    public int? FkTaskId { get; set; }

    public bool? IsRead { get; set; }

    public int? FkUserId { get; set; }

    public int? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual TaskAssign? FkTask { get; set; }

    public virtual User? FkUser { get; set; }
}
