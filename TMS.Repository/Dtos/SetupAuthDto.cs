using TMS.Repository.Enums;

namespace TMS.Repository.Dtos;

public class SetupAuthDto
{
    public string Email { get; set; }
    public int AuthType { get; set; }
    public bool IsEnabled { get; set; }
}
