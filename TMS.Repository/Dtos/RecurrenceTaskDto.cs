using System.Text.Json;

namespace TMS.Repository.Dtos;

public class RecurrenceTaskDto
{
    public string? Id { get; set; }

    public int? FkUserId { get; set; }

    public string? UserName { get; set; }

    public string? TaskName { get; set; }

    public string? SubTaskName { get; set; }

    public List<RecurrenceTaskActionDto>? TaskActionList { get; set; }
}
