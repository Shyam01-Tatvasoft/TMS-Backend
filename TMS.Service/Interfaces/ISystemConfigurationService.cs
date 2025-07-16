using TMS.Repository.Dtos;

namespace TMS.Service.Interfaces;

public interface ISystemConfigurationService
{
    public Task<SystemConfigurationDto> GetAllSystemConfiguration();
    public System.Threading.Tasks.Task UpdateSystemConfiguration(List<ConfigurationDto> systemConfigs);
}
