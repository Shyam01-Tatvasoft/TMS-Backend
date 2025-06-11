using System;
using System.Collections.Generic;

namespace TMS.Repository.Data;

public partial class User
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Password { get; set; }

    public string? Phone { get; set; }

    public int? FkCountryId { get; set; }

    public int? FkCountryTimezone { get; set; }

    public int? FkRoleId { get; set; }

    public string Username { get; set; } = null!;

    public bool? IsDeleted { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public virtual Country? FkCountry { get; set; }

    public virtual TimezoneDetail? FkCountryTimezoneNavigation { get; set; }

    public virtual Role? FkRole { get; set; }
}
