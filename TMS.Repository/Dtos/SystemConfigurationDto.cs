namespace TMS.Repository.Dtos;

public class SystemConfigurationDto
{
    public List<ConfigurationDto>? EmailSettings { get; set; }
    public List<ConfigurationDto>? LoginSettings { get; set; }
    public List<ConfigurationDto>? ExternalSettings { get; set; }
}
