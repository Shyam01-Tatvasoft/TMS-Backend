using TMS.Repository.Data;
using TMS.Repository.Dtos;

namespace TMS.Service.Interfaces;

public interface ITaskService
{
    public Task<(List<TaskAssignDto>, int count)> GetAllTaskAssignAsync(int id, string role, string? taskType, int? statusFilter, int? userFilter, int skip, int take, string? search, string? sorting, string? sortDirection);
    public Task<UpdateTaskDto?> GetTaskAssignAsync(int id);
    public Task<(int id, string message)> AddTaskAssignAsync(AddTaskDto task, string role);
    public Task<(bool success, string message)> UpdateTaskAssignAsync(EditTaskDto task, string role);
    public Task<List<SubTaskDto>> GetSubTasksByTaskIdAsync(int id);
    public Task<List<TaskDto>> GetAllTasksAsync();
    public Task<TaskAssign> ApproveTask(int id);
    public Task<TaskAssign> ReassignTask(ReassignTaskDto dto);
    public Task<List<TaskAssignDto>> GetTasksForSchedular(DateTime start, DateTime end, string role, int userId);
    public Task<List<TaskGraphDto>> GetTaskChartData(TaskGraphFilterDto filter);
    public Task<(bool, string)> DeleteUpcomingRecurrenceTask(string recurrenceId);
    public Task<(bool, string)> DeleteRecurrence(string recurrenceId);
    public Task<List<StatusChartResultDto>> GetStatusChartData(StatusChartFilterDto filter);
}
