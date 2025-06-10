using System;
using System.Collections.Generic;

namespace TMS.Repository.Data;

public partial class Timezone
{
    public int Id { get; set; }

    public string TimezoneName { get; set; } = null!;

    public int? FkCountryId { get; set; }

    public string? ZoneName { get; set; }

    public string? GmtOffsetName { get; set; }

    public virtual Country? FkCountry { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
