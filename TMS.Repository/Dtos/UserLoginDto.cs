namespace TMS.Repository.Dtos;

public class UserLoginDto
{
    public string Email { get; set; } = null!;
    public string Password { get; set; }
    public bool RememberMe { get; set; }
}
