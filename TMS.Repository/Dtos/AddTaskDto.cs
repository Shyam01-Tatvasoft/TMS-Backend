using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace TMS.Repository.Dtos;

public class AddTaskDto
{
    public string? Description { get; set; }

    [Required]
    public int? FkUserId { get; set; }

    [Required]
    public int? FkTaskId { get; set; }

    [Required]
    public int? FkSubtaskId { get; set; }

    [Required]
    public JsonElement? TaskData { get; set; }

    [Required]
    public DateTime DueDate { get; set; }

    [Required]
    public int? Status { get; set; }

    [Required]
    public int? Priority { get; set; }

    public bool Recurrence { get; set; }

    public int? RecurrencePattern { get; set; }

    public int? RecurrenceOn { get; set; }

    public int? EndAfter { get; set; }

    public DateTime RecurrenceTo { get; set; }
}
