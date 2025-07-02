namespace TMS.Repository.Dtos;

public class TaskGraphFilterDto
{
    public int? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? DateRange { get; set; }
}
