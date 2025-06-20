using System.Text.Json;

namespace TMS.Repository.Dtos;

public class TaskActionDto
{
    public int Id { get; set; }

    public int? FkTaskId { get; set; }

    public int? FkUserId { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public JsonElement? SubmittedData { get; set; }

    public string? UserName { get; set; }

    public string? TaskName { get; set; }

    public string? SubTaskName { get; set; }

    public string Status { get; set; } = string.Empty;

}
