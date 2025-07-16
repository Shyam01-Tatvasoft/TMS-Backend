using TMS.Repository.Data;

namespace TMS.Repository.Interfaces;

public interface ISystemConfigurationRepository
{
    public Task<List<SystemConfiguration>> GetAllConfigsAsync();
    public System.Threading.Tasks.Task RefreshCacheAsync();
    public Task<bool> AddSystemConfig(SystemConfiguration newConfig);
    public System.Threading.Tasks.Task UpdateSystemConfig(SystemConfiguration updatedConfig);
    public Task<string?> GetConfigByNameAsync(string configName);
}
