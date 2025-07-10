namespace TMS.Repository.Dtos;

public class Login2FaDto
{
    public string Email { get; set; } = null!;
    public string Code { get; set; } = null!;
}
