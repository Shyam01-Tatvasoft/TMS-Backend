namespace TMS.Repository.Dtos;

public class EmailConfigDto
{
    public string SenderEmail { get; set; } = string.Empty;
    public string EmailHost { get; set; } = string.Empty;
    public string SenderPassword { get; set; } = string.Empty;
    public int Port { get; set; }
}
