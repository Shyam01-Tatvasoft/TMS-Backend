namespace TMS.Repository.Dtos;

public class SubTaskDto
{
    public int Id { get; set; }

    public int? FkTaskId { get; set; }

    public string Name { get; set; } = null!;
}
