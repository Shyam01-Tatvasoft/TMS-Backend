using System;
using System.Collections.Generic;

namespace TMS.Repository.Data;

public partial class SystemConfiguration
{
    public int Id { get; set; }

    public string ConfigName { get; set; } = null!;

    public string ConfigValue { get; set; } = null!;

    public DateTime? UpdatedAt { get; set; }
}
