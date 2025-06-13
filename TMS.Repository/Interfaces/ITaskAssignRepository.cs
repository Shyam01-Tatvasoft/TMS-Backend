using TMS.Repository.Data;

namespace TMS.Repository.Interfaces;

public interface ITaskAssignRepository
{
    public Task<List<TaskAssign>> GetAllTaskAssignAsync(int id, string role);
    public Task<TaskAssign?> GetTaskAssignAsync(int id);
    public System.Threading.Tasks.Task AddTaskAssignAsync(TaskAssign task);
    public System.Threading.Tasks.Task UpdateTaskAssignAsync(TaskAssign task);
}
