using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace TMS.Repository.Dtos;

public class UserDto
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Password { get; set; }

    public string? Phone { get; set; }

    public string? CountryName { get; set; }

    public string? TimezoneName { get; set; }

    public int? FkCountryId { get; set; }

    public int? FkCountryTimezone { get; set; }

    public string? Role { get; set; }

    public bool? IsDeleted { get; set; }

    public string Username { get; set; } = null!;

    public string? ProfileImagePath { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public bool? IsTwoFaEnabled { get; set; }

    public int? AuthType { get; set; }

    public bool HasSecret { get; set; }
}
