using TMS.Repository.Data;
using TMS.Repository.Dtos;

namespace TMS.Repository.Interfaces;

public interface ILogRepository
{
    public Task<int> AddLogAsync(Log log);
    public Task<(List<LogDto>,int count)> GetAllLogsAsync(int skip, int take, string? search, string? sorting, string? sortDirection, string filterBy);
    public Task<Log?> GetLogByIdAsync(int id);
}
