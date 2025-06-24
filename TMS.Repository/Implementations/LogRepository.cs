using Microsoft.EntityFrameworkCore;
using TMS.Repository.Data;
using TMS.Repository.Interfaces;

namespace TMS.Repository.Implementations;

public class LogRepository: ILogRepository
{
    private readonly TmsContext _context;

    public LogRepository(TmsContext context)
    {
        _context = context;
    }

    public async Task<int> AddLogAsync(Log log)
    {
        _context.Logs.Add(log);
        await _context.SaveChangesAsync();
        return log.Id;
    }

    public async Task<List<Log>> GetAllLogsAsync()
    {
        return await _context.Logs.ToListAsync();
    }
}
