using TMS.Repository.Data;

namespace TMS.Repository.Interfaces;

public interface ISystemConfigurationRepository
{
    public Task<List<SystemConfiguration>> GetAllConfigsAsync();
    public Task<string?> GetValueAsync(string key);
    public System.Threading.Tasks.Task RefreshCacheAsync();
}
