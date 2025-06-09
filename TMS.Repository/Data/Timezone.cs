using System;
using System.Collections.Generic;

namespace TMS.Repository.Data;

public partial class Timezone
{
    public int Id { get; set; }

    public string TimezoneName { get; set; } = null!;

    public int? CountryId { get; set; }

    public virtual Country? Country { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
