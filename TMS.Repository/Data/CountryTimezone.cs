using System;
using System.Collections.Generic;

namespace TMS.Repository.Data;

public partial class CountryTimezone
{
    public int Id { get; set; }

    public string Timezone { get; set; } = null!;

    public int? FkCountryId { get; set; }

    public string? Zone { get; set; }

    public string? Offset { get; set; }

    public virtual Country? FkCountry { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
