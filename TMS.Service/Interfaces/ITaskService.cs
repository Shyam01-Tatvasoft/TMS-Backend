using TMS.Repository.Data;
using TMS.Repository.Dtos;

namespace TMS.Service.Interfaces;

public interface ITaskService
{
    public Task<List<TaskAssignDto>> GetAllTaskAssignAsync(int id, string role);
    public Task<UpdateTaskDto?> GetTaskAssignAsync(int id);
    public Task<(int id, string message)> AddTaskAssignAsync(AddTaskDto task);
    public Task<(bool success, string message)> UpdateTaskAssignAsync(EditTaskDto task);
    public Task<List<SubTaskDto>> GetSubTasksByTaskIdAsync(int id);
    public Task<List<TaskDto>> GetAllTasksAsync();
}
