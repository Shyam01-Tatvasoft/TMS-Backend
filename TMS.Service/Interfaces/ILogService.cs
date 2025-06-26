using TMS.Repository.Data;
using TMS.Repository.Dtos;

namespace TMS.Service.Interfaces;

public interface ILogService
{
    public Task<int> LogAsync(string? message, int? userId, string action, string? stackTrash, string? data);
    public Task<(List<LogDto>,int count)> GetAllLogsAsync(int skip, int take, string? search, string? sorting, string? sortDirection, string filterBy);
    public Task<LogDto?> GetLogByIdAsync(int id);
}
