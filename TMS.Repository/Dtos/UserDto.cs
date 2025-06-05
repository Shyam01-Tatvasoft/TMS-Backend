namespace TMS.Repository.Dtos;

public class UserDto
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Password { get; set; }

    public string? Phone { get; set; }

    public string? Country { get; set; }

    public string? Role { get; set; }
}
