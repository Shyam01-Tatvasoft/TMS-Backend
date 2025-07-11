namespace TMS.Repository.Dtos;

public class TaskFilterDto
{
    public string? TaskType { get; set; }
    public int? StatusFilter { get; set; }
    public int? UserFilter { get; set; }
    public int? PriorityFilter { get; set; }
}
