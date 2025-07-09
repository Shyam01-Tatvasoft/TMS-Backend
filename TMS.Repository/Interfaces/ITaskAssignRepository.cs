using TMS.Repository.Data;
using TMS.Repository.Dtos;

namespace TMS.Repository.Interfaces;

public interface ITaskAssignRepository
{
    public Task<(List<TaskAssignDto>, int count)> GetAllTaskAssignAsync(int id, string role, int skip, int take, string? search, string? sorting, string? sortDirection, string? taskType, int? statusFilter, int? userFilter);
    public Task<TaskAssign?> GetTaskAssignAsync(int id);
    public System.Threading.Tasks.Task AddTaskAssignAsync(TaskAssign task);
    public System.Threading.Tasks.Task UpdateTaskAssignAsync(TaskAssign task);
    public Task<List<TaskAssignDto>> GetTasksForSchedular(DateTime start, DateTime end, string role, int userId);
    public Task<List<TaskAssign>> GetDueTasksAsync();
    public Task<List<TaskAssign>> GetOverdueTasksAsync();
    public Task<List<TaskAssign>> GetTodaysRecurrentTasksAsync();
    public Task<List<TaskGraphDto>> GetTaskChartDataAsync(TaskGraphFilterDto filter);
    public Task<List<TaskAssign>> GetRecurrenceTaskAsync(string recurrenceId);
    public Task<List<StatusChartResultDto>> GetStatusChartAsync(StatusChartFilterDto filter);
}
