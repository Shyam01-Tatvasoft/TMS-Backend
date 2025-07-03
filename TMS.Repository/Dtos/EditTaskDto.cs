using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace TMS.Repository.Dtos;

public class EditTaskDto
{
    [Required]
    public int Id { get; set; }

    public string? Description { get; set; }
    
    [Required]
    public JsonElement? TaskData { get; set; }

    [Required]
    public DateTime DueDate { get; set; }

    [Required]
    public int? Status { get; set; }

    [Required]
    public int? Priority { get; set; }

    [Required]
    public int? EndAfter { get; set; }
}
