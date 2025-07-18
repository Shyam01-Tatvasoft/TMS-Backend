using System;
using System.Collections.Generic;

namespace TMS.Repository.Data;

public partial class EmailTemplate
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Body { get; set; } = null!;
}
