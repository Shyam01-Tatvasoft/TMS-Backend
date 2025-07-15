using TMS.Repository.Data;
using TMS.Repository.Dtos;
using TMS.Repository.Interfaces;
using TMS.Service.Interfaces;

namespace TMS.Service;

public class SystemConfigurationService: ISystemConfigurationService
{
    private readonly ISystemConfigurationRepository _systemConfigRepository;

    public SystemConfigurationService(ISystemConfigurationRepository systemConfigRepository)
    {
        _systemConfigRepository = systemConfigRepository;
    }

    public async Task<SystemConfigurationDto> GetAllSystemConfiguration()
    {
       List<SystemConfiguration> configs = await _systemConfigRepository.GetAllConfigsAsync();
       
       SystemConfigurationDto systemConfigs = new();
       return systemConfigs;
    }
}
