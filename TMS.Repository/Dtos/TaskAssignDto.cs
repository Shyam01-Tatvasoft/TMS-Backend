using System.Text.Json;

namespace TMS.Repository.Dtos;

public class TaskAssignDto
{
    public int Id { get; set; }

    public string? Description { get; set; }

    public string? UserName { get; set; }

    public JsonElement? TaskData { get; set; }

    public DateTime DueDate { get; set; }

    public string? Status { get; set; }

    public string? Priority { get; set; }

    public DateTime? CreatedAt { get; set; }

   public string TaskName { get; set; } = string.Empty;
   
   public string SubTaskName { get; set; } = string.Empty;
}
