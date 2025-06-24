using TMS.Repository.Data;
using TMS.Repository.Interfaces;
using TMS.Service.Interfaces;

namespace TMS.Service.Implementations;

public class LogService : ILogService
{

    private readonly ILogRepository _logRepository;
    public LogService(ILogRepository logRepository)
    {
        _logRepository = logRepository;
    }

    public async Task<int> LogAsync(string? message, int userId, string action, string? stackTrash, string? data)
    {
        Log logEntry = new Log
        {
            Message = message,
            FkUserId = userId,
            Date = DateTime.Now,
            Action = action,
            Data = data,
            Stacktrash = stackTrash
        };

        return await _logRepository.AddLogAsync(logEntry);
    }
}
