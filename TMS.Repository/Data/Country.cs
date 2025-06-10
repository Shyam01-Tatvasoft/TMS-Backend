using System;
using System.Collections.Generic;

namespace TMS.Repository.Data;

public partial class Country
{
    public int Id { get; set; }

    public string CountryName { get; set; } = null!;

    public string IsoCode { get; set; } = null!;

    public string? Flag { get; set; }

    public string? PhoneCode { get; set; }

    public virtual ICollection<Timezone> Timezones { get; set; } = new List<Timezone>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
