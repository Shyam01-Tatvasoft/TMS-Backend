using TMS.Repository.Data;

namespace TMS.Repository.Interfaces;

public interface ILogRepository
{
    public Task<int> AddLogAsync(Log log);
    public Task<List<Log>> GetAllLogsAsync();
}
