using System;
using System.Collections.Generic;

namespace TMS.Repository.Data;

public partial class Country
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string IsoCode { get; set; } = null!;

    public string? Flag { get; set; }

    public string? PhoneCode { get; set; }

    public virtual ICollection<TimezoneDetail> TimezoneDetails { get; set; } = new List<TimezoneDetail>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
