namespace TMS.Repository.Dtos;

public class ReassignTaskDto
{
    public int TaskId { get; set; }
    public int UserId { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Comments { get; set; } = string.Empty;
}
