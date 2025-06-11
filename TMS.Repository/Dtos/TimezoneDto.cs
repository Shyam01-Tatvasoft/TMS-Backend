namespace TMS.Repository.Dtos;

public class TimezoneDto
{
    public string zoneName { get; set; }
    public int gmtOffset { get; set; }
    public string gmtOffsetName { get; set; }
    public string abbreviation { get; set; }
    public string tzName { get; set; }
}
