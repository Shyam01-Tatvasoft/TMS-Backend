using System.Text.Json;

namespace TMS.Repository.Dtos;

public class RecurrenceTaskActionDto
{
    public int TaskId { get; set; }

    public int? TaskActionId { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime? AssignedAt { get; set; }
    
    public DateTime? SubmittedAt { get; set; }

    public JsonElement? SubmittedData { get; set; }

    public string Status { get; set; } = string.Empty;
}
