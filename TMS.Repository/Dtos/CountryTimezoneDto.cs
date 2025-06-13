namespace TMS.Repository.Dtos;

public class CountryTimezoneDto
{
    public int Id { get; set; }

    public string Timezone { get; set; } = null!;

    public int? FkCountryId { get; set; }

    public string? Zone { get; set; }

    public string? Offset { get; set; }
}
