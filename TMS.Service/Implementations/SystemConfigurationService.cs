using TMS.Repository.Data;
using TMS.Repository.Dtos;
using TMS.Repository.Interfaces;
using TMS.Service.Interfaces;

namespace TMS.Service;

public class SystemConfigurationService : ISystemConfigurationService
{
    private readonly ISystemConfigurationRepository _systemConfigRepository;

    public SystemConfigurationService(ISystemConfigurationRepository systemConfigRepository)
    {
        _systemConfigRepository = systemConfigRepository;
    }

    public async Task<SystemConfigurationDto> GetAllSystemConfiguration()
    {
        List<SystemConfiguration> configs = await _systemConfigRepository.GetAllConfigsAsync();

        EmailConfigDto emailConfigs = new()
        {
            SenderEmail = configs.Find(c => c.ConfigName == "SenderEmail")?.ConfigValue!,
            EmailHost = configs.Find(c => c.ConfigName == "EmailHost")?.ConfigValue!,
            Port = int.Parse(configs.Find(c => c.ConfigName == "Port")?.ConfigValue!),
            SenderPassword = configs.Find(c => c.ConfigName == "SenderPassword")?.ConfigValue!
        };

        LoginConfigDto loginConfigs = new()
        {
            JWTSecret = configs.Find(c => c.ConfigName == "JWTSecret")?.ConfigValue!,
            JWTIssuer = configs.Find(c => c.ConfigName == "JWTIssuer")?.ConfigValue!,
            JWTAudience = configs.Find(c => c.ConfigName == "JWTAudience")?.ConfigValue!,
            JWTSubject = configs.Find(c => c.ConfigName == "JWTSubject")?.ConfigValue!,
            JWTExpiry = int.Parse(configs.Find(c => c.ConfigName == "JWTExpiry")?.ConfigValue!),
            JWTExpiryLong = int.Parse(configs.Find(c => c.ConfigName == "JWTExpiryLong")?.ConfigValue!),
            LockupDuration = int.Parse(configs.Find(c => c.ConfigName == "LockupDuration")?.ConfigValue!),
            UserLockup = int.Parse(configs.Find(c => c.ConfigName == "UserLockup")?.ConfigValue!),
            ResetPasswordLinkExpiry = int.Parse(configs.Find(c => c.ConfigName == "ResetPasswordLinkExpiry")?.ConfigValue!),
            SetupPasswordLinkExpiry = int.Parse(configs.Find(c => c.ConfigName == "SetupPasswordLinkExpiry")?.ConfigValue!),
            PasswordExpiryDuration = int.Parse(configs.Find(c => c.ConfigName == "PasswordExpiryDuration")?.ConfigValue!)
        };

        ExternalConfigDto externalConfigs = new()
        {
            HolidayApi = configs.Find(c => c.ConfigName == "HolidayApi")?.ConfigValue!,
        };

        SystemConfigurationDto systemConfigs = new()
        {
            EmailConfigs = emailConfigs,
            LoginConfigs = loginConfigs,
            ExternalConfigs = externalConfigs
        };
       return systemConfigs;
    }

    public async System.Threading.Tasks.Task UpdateSystemConfiguration(List<ConfigurationDto> systemConfigs)
    {
        List<SystemConfiguration> configs = await _systemConfigRepository.GetAllConfigsAsync();
        foreach (var systemConfigDto in systemConfigs)  
        {
            var name = systemConfigDto.Name;
            var newName = char.ToUpper(name[0]) + name.Substring(1);
            var existingConfig = configs.Find(c => c.ConfigName == newName);
            if( existingConfig != null)
            {
                existingConfig.ConfigValue = systemConfigDto.Value;
                existingConfig.UpdatedAt = DateTime.Now;
                await _systemConfigRepository.UpdateSystemConfig(existingConfig);
            }
            else
            {
                SystemConfiguration newConfig = new()
                {
                    ConfigName = systemConfigDto.Name,
                    ConfigValue = systemConfigDto.Value,
                };
                await _systemConfigRepository.AddSystemConfig(newConfig);
            }
        }

        await _systemConfigRepository.RefreshCacheAsync();
    }
}
