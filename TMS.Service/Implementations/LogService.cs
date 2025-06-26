using System.Text.Json;
using TMS.Repository.Data;
using TMS.Repository.Dtos;
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

    public async Task<int> LogAsync(string? message, int? userId, string action, string? stackTrash, string? data)
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

    public async Task<(List<LogDto>,int count)> GetAllLogsAsync(int skip, int take, string? search, string? sorting, string? sortDirection, string filterBy)
    {
        return await _logRepository.GetAllLogsAsync(skip, take, search, sorting, sortDirection, filterBy);
    }

    public async Task<LogDto?> GetLogByIdAsync(int id)
    {
        Log? log =  await _logRepository.GetLogByIdAsync(id);
        if(log == null)
        {
            return null;
        }
        if(log.Message == "Add User" || log.Message == "Get user" ||log.Message == "Update User")
        {
            log.Data = MaskUserData(log.Data);
        }


        LogDto newLog =  new LogDto
        {
            Id = log.Id,
            Message = log.Message,
            FkUserId = log.FkUserId == 1 ? "Admin" : "User",
            Date = log.Date,
            Action = log.Action,
            Data = JsonSerializer.Deserialize<JsonElement>(log.Data!),
            StackTrash = log.Stacktrash
        };

        return newLog;
    }

    private string? MaskUserData(string? jsonData)
    {
        if (string.IsNullOrEmpty(jsonData))
        {
            return null;
        }

        var jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData);
        if (jsonObject != null)
        {
            foreach (var key in jsonObject.Keys.ToList())
            {
                // make one function which mask all fields value like update first and last character with -- / xx 
                if (key.Equals("firstName", StringComparison.OrdinalIgnoreCase) || 
                    key.Equals("lastName", StringComparison.OrdinalIgnoreCase) || 
                    key.Equals("email", StringComparison.OrdinalIgnoreCase) || 
                    key.Equals("phone", StringComparison.OrdinalIgnoreCase) ||
                    key.Equals("password", StringComparison.OrdinalIgnoreCase) ||
                    key.Equals("username", StringComparison.OrdinalIgnoreCase))
                {
                    jsonObject[key] = maskAllValues(jsonObject[key]?.ToString());
                }
            }
        }
        return Newtonsoft.Json.JsonConvert.SerializeObject(jsonObject);
    }
    

    private string maskAllValues(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }
        return value.Length > 2 ? value[..1] + new string('#', value.Length - 2) + value[^1..] : new string('#', value.Length);
    }
}
