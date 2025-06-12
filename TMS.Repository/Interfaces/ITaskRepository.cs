namespace TMS.Repository.Interfaces;

public interface ITaskRepository
{
    public Task<List<TMS.Repository.Data.Task>> GetAllTasksAsync();
    public Task<List<TMS.Repository.Data.SubTask>> GetSubTasksByTaskIdAsync(int id);
}
