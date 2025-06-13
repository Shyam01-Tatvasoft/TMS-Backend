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
}
