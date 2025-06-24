namespace TMS.Repository.Dtos;

public class NotificationDto
{
    public int Id { get; set; }

    public int? FkTaskId { get; set; }

    public int? FkUserId { get; set; }

    public string? TaskType { get; set; }

    public string TaskDescription { get; set; } = string.Empty;

    public string? Priority { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public bool? IsRead { get; set; }

    public string? UserName { get; set; }

    public DateTime? CreatedAt { get; set; }
}
