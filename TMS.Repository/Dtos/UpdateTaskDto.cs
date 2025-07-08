using System.Text.Json;

namespace TMS.Repository.Dtos;

public class UpdateTaskDto
{
    public int Id { get; set; }
    public int? FkUserId { get; set; }
    public int? FkTaskId { get; set; }
    public int? FkSubTaskId { get; set; }
    public JsonElement? TaskData { get; set; }
    public DateTime DueDate { get; set; }
    public int? Status { get; set; }
    public int? Priority { get; set; }
    public string? Description { get; set; }
    public int? FkTaskActionId { get; set; }
    public bool? IsRecurrence { get; set; }
    public string? RecurrenceId { get; set; }
    public string? RecurrencePattern { get; set; }
    public int? RecurrenceOn { get; set; }
    public int? EndAfter { get; set; }
    public DateTime? RecurrenceTo { get; set; }
}
