namespace TMS.Repository.Dtos;

public class StatusChartFilterDto
{
    public int? UserId { get; set; }
    public int? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
