using TMS.Repository.Data;

namespace TMS.Repository.Interfaces;

public interface ITaskActionRepository
{
    public Task<List<TaskAction>> GetAllTaskActionsAsync();
    public Task<TaskAction?> GetTaskActionByIdAsync(int id);
    public Task<TaskAction> AddTaskActionAsync(TaskAction taskAction);
}
