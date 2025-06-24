namespace TMS.Service.Interfaces;

public interface ILogService
{
    public Task<int> LogAsync(string? message, int userId, string action, string? stackTrash, string? data);
}
