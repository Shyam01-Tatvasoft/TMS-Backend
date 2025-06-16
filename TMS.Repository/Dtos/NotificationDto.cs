namespace TMS.Repository.Dtos;

public class NotificationDto
{
    public int Id { get; set; }

    public int? FkTaskId { get; set; }

    public int? FkUserId { get; set; }

    public string? TaskType { get; set; }

    public string TaskDescription { get; set; }
    
    public bool? IsRead { get; set; }
}
