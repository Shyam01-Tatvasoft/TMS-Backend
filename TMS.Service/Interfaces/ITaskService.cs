using TMS.Repository.Data;
using TMS.Repository.Dtos;

namespace TMS.Service.Interfaces;

public interface ITaskService
{
    public Task<List<TaskAssignDto>> GetAllTaskAssignAsync();
    public Task<TaskAssignDto?> GetTaskAssignAsync(int id);
    public Task<(bool success,string message)> AddTaskAssignAsync(AddEditTaskDto task);
    public Task<(bool success, string message)> UpdateTaskAssignAsync(AddEditTaskDto task);
    public Task<List<SubTask>> GetSubTasksByTaskIdAsync(int id);
    public Task<List<Repository.Data.Task>> GetAllTasksAsync();
}
