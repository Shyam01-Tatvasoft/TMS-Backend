namespace TMS.Repository.Dtos;

public class CountryDto
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string IsoCode { get; set; } = null!;

    public string? Flag { get; set; }

    public string? PhoneCode { get; set; }

    public List<TimezoneDto>? timezones { get; set; }
}
