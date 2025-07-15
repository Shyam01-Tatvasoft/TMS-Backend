using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using TMS.Repository.Data;
using TMS.Repository.Interfaces;

namespace TMS.Repository.Implementations;

public class SystemConfigurationRepository: ISystemConfigurationRepository
{
    private readonly TmsContext _context;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;
    private const string ConfigCacheKey = "SystemConfigurations";
    
    public SystemConfigurationRepository(TmsContext context, IMemoryCache cache, IConfiguration configuration)
    {
        _context = context;
        _cache = cache;
        _configuration = configuration;
    }

    public async Task<List<SystemConfiguration>> GetAllConfigsAsync()
    {
        if (!_cache.TryGetValue(ConfigCacheKey, out List<SystemConfiguration>? configs))
        {
            configs = await _context.SystemConfigurations.ToListAsync();
            _cache.Set(ConfigCacheKey, configs, TimeSpan.FromMinutes(30));
        }
        return configs!;
    }

    public async Task<string?> GetValueAsync(string key)
    {
        var configs = await GetAllConfigsAsync();
        var config = configs.FirstOrDefault(x => x.ConfigName == key);

        if (config == null) return null;
        return config.ConfigValue;
    }

    public async System.Threading.Tasks.Task RefreshCacheAsync()
    {
        var configs = await _context.SystemConfigurations.ToListAsync();
        _cache.Set(ConfigCacheKey, configs, TimeSpan.FromMinutes(30));
    }

}
