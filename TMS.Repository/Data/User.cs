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

    public string? ProfileImage { get; set; }

    public bool? IsTwoFaEnabled { get; set; }

    public int? AuthType { get; set; }

    public virtual Country? FkCountry { get; set; }

    public virtual CountryTimezone? FkCountryTimezoneNavigation { get; set; }

    public virtual Role? FkRole { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<TaskAction> TaskActions { get; set; } = new List<TaskAction>();

    public virtual ICollection<TaskAssign> TaskAssigns { get; set; } = new List<TaskAssign>();
}
