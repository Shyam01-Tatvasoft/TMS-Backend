using TMS.Repository.Data;
using TMS.Repository.Dtos;

namespace TMS.Service.Interfaces;

public interface ITaskActionService
{
    public Task<List<TaskAction>> GetAllTaskActionsAsync();
    public Task<TaskAction?> GetTaskActionByIdAsync(int id);
    public Task<int> AddTaskActionAsync(EmailTaskDto emailTask);
    public Task<int> AddUploadTaskAsync(UploadFileTaskDto dto);
}
