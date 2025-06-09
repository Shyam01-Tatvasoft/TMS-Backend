using System;
using System.Collections.Generic;

namespace TMS.Repository.Data;

public partial class Country
{
    public int Id { get; set; }

    public string CountryName { get; set; } = null!;

    public string CountryCode { get; set; } = null!;

    public virtual ICollection<Timezone> Timezones { get; set; } = new List<Timezone>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
