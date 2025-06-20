using System.ComponentModel.DataAnnotations;

namespace TMS.Repository.Dtos;

public class EmailTaskDto
{
    [Required]
    public int FkTaskId { get; set; }
    [Required]
    public int FkUserId { get; set; }
    public int? TaskActionId { get; set; }
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string Subject { get; set; } = string.Empty;
}
