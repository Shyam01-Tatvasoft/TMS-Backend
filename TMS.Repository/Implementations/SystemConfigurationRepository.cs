using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using TMS.Repository.Data;
using TMS.Repository.Interfaces;

namespace TMS.Repository.Implementations;

public class SystemConfigurationRepository : ISystemConfigurationRepository
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
            _cache.Set(ConfigCacheKey, configs);
        }
        return configs!;
    }

    public async Task<string?> GetConfigByNameAsync(string configName)
    {
        if (!_cache.TryGetValue(ConfigCacheKey, out List<SystemConfiguration>? configs))
        {
            configs = await _context.SystemConfigurations.ToListAsync();
            _cache.Set(ConfigCacheKey, configs);
        }
        return configs?.FirstOrDefault(c => c.ConfigName == configName)?.ConfigValue;
    }

    public async System.Threading.Tasks.Task RefreshCacheAsync()
    {
        var configs = await _context.SystemConfigurations.ToListAsync();
        _cache.Set(ConfigCacheKey, configs);
    }

    public async Task<bool> AddSystemConfig(SystemConfiguration newConfig)
    {
        await _context.AddAsync(newConfig);
        await _context.SaveChangesAsync();
        return true;
    }

    public async System.Threading.Tasks.Task UpdateSystemConfig(SystemConfiguration updatedConfig)
    {
        _context.Update(updatedConfig);
        await _context.SaveChangesAsync();
    }
}
