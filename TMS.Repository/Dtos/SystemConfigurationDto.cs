namespace TMS.Repository.Dtos;

public class SystemConfigurationDto
{
    public EmailConfigDto? EmailConfigs { get; set; }
    public LoginConfigDto? LoginConfigs { get; set; }
    public ExternalConfigDto? ExternalConfigs { get; set; }
}