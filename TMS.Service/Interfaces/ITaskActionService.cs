using TMS.Repository.Data;
using TMS.Repository.Dtos;

namespace TMS.Service.Interfaces;

public interface ITaskActionService
{
    public Task<List<TaskAction>> GetAllTaskActionsAsync();
    public Task<TaskActionDto?> GetTaskActionByIdAsync(int id);
    public Task<int> AddEmailTaskAsync(EmailTaskDto emailTask);
    public Task<int> AddUploadTaskAsync(UploadFileTaskDto dto);
    public Task<List<TaskFileData>?> GetTaskFileData(int id);
}
