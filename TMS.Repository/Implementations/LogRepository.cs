using Microsoft.EntityFrameworkCore;
using TMS.Repository.Data;
using TMS.Repository.Dtos;
using TMS.Repository.Interfaces;

namespace TMS.Repository.Implementations;

public class LogRepository : ILogRepository
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

    public async Task<(List<LogDto>, int count)> GetAllLogsAsync(int skip, int take, string? search, string? sorting, string? sortDirection, string filterBy)
    {
        var query = _context.Logs
            .AsQueryable();

        if (!string.IsNullOrEmpty(filterBy))
        {
            filterBy = filterBy.ToLower();
            query = filterBy switch
            {
                "exception" => query.Where(l => l.Action == "Exception"),
                "actions" => query.Where(l => l.Action != "Exception"),
                _ => query
            };
        }

        if (!string.IsNullOrEmpty(search))
        {
            search = search.ToLower();
            query = query.Where(l => l.Message.ToLower().Contains(search) ||
                                     l.Action.ToLower().Contains(search) ||
                                     l.Data.ToLower().Contains(search));
        }

        if (!string.IsNullOrEmpty(sorting) && !string.IsNullOrEmpty(sortDirection))
        {
            if (sortDirection.ToLower() == "asc")
            {
                query = sorting switch
                {
                    "1" => query.OrderBy(l => l.Date),
                    _ => query.OrderBy(l => l.Id)
                };
            }
            else
            {
                query = sorting switch
                {
                    "1" => query.OrderByDescending(l => l.Date),
                    _ => query.OrderByDescending(l => l.Id)
                };
            }
        }
        else
        {
            query = query.OrderByDescending(l => l.Id);
        }
        int count = await query.CountAsync();
        List<LogDto> logs = await query.Skip(skip).Take(take)
        .Select(l => new LogDto
        {
            Id = l.Id,
            Message = l.Message,
            Date = l.Date,
            Action = l.Action,
            Data = null,
            StackTrash = l.Stacktrash,
            FkUserId = l.FkUserId == 1 ? "Admin" : "User",
        }).ToListAsync();
        return (logs, count);
    }

    public async Task<Log?> GetLogByIdAsync(int id)
    {
        return await _context.Logs
            .FirstOrDefaultAsync(l => l.Id == id);
    }
}
