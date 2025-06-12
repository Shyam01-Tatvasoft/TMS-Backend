using System.Text.Json;
using TMS.Repository.Data;
using TMS.Repository.Dtos;
using TMS.Repository.Interfaces;
using TMS.Service.Interfaces;

namespace TMS.Service.Implementations;

public class TaskService : ITaskService
{
    private readonly ITaskAssignRepository _taskAssignRepository;
    private readonly ITaskRepository _taskRepository;

    public TaskService(ITaskAssignRepository taskAssignRepository, ITaskRepository taskRepository)
    {
        _taskAssignRepository = taskAssignRepository;
        _taskRepository = taskRepository;
    }

    public async Task<List<Repository.Data.Task>> GetAllTasksAsync()
    {
        return await _taskRepository.GetAllTasksAsync();
    }

    public async Task<List<SubTask>> GetSubTasksByTaskIdAsync(int id)
    {
        return await _taskRepository.GetSubTasksByTaskIdAsync(id);
    }

    public async Task<List<TaskAssignDto>> GetAllTaskAssignAsync()
    {
        List<TaskAssign> TaskAssignList = await _taskAssignRepository.GetAllTaskAssignAsync();
        List<TaskAssignDto> taskAssignDtos = TaskAssignList.Select(taskAssign => new TaskAssignDto
        {
            Id = taskAssign.Id,
            UserName = taskAssign?.FkUser?.FirstName + " " + taskAssign?.FkUser?.FirstName,
            Description = taskAssign.Description,
            TaskData = !string.IsNullOrEmpty(taskAssign.TaskData) ? JsonSerializer.Deserialize<JsonElement>(taskAssign.TaskData!) : null,
            DueDate = taskAssign.DueDate,
            Status = taskAssign.Status,
            Priority = taskAssign.Priority,
            CreatedAt = taskAssign.CreatedAt,
            TaskName = taskAssign.FkTask?.Name ?? string.Empty,
            SubTaskName = taskAssign.FkSubtask?.Name ?? string.Empty
        }).ToList();

        return taskAssignDtos;
    }

    public async Task<TaskAssignDto?> GetTaskAssignAsync(int id)
    {
        TaskAssign taskAssign =  await _taskAssignRepository.GetTaskAssignAsync(id);
        TaskAssignDto TaskAssignDto = new()
        {
            Id = taskAssign.Id,
            UserName = taskAssign?.FkUser?.FirstName + " " + taskAssign?.FkUser?.FirstName,
            Description = taskAssign.Description,
            TaskData = !string.IsNullOrEmpty(taskAssign.TaskData) ? JsonSerializer.Deserialize<JsonElement>(taskAssign.TaskData!) : null,
            DueDate = taskAssign.DueDate,
            Status = taskAssign.Status,
            Priority = taskAssign.Priority,
            CreatedAt = taskAssign.CreatedAt,
            TaskName = taskAssign.FkTask?.Name ?? string.Empty,
            SubTaskName = taskAssign.FkSubtask?.Name ?? string.Empty
        };

        return TaskAssignDto;
    }

    public async Task<(bool success,string message)> AddTaskAssignAsync(AddEditTaskDto task)
    {
        TaskAssign newTask = new()
        {
            Description = task.Description,
            FkUserId = task.FkUserId,
            FkTaskId = task.FkTaskId,
            FkSubtaskId = task.FkSubtaskId,
            TaskData = JsonSerializer.Serialize(task.TaskData), // retains original formatting
            DueDate = task.DueDate,
            Status = task.Status,
            Priority = task.Priority,
            CreatedAt = DateTime.UtcNow,
        };
        await _taskAssignRepository.AddTaskAssignAsync(newTask);

        return (true, "Task assigned successfully.");
    }

    public async Task<(bool success, string message)> UpdateTaskAssignAsync(AddEditTaskDto task)
    {
        TaskAssign? existingTask = await _taskAssignRepository.GetTaskAssignAsync(task.Id);
        if (existingTask == null)
        {
            return (false, "Task assign not found.");
        }

        existingTask.Description = task.Description;
        existingTask.FkUserId = task.FkUserId;
        existingTask.FkTaskId = task.FkTaskId;
        existingTask.FkSubtaskId = task.FkSubtaskId;
        existingTask.TaskData = JsonSerializer.Serialize(task.TaskData);
        existingTask.DueDate = task.DueDate;
        existingTask.Status = task.Status;
        existingTask.Priority = task.Priority;

        await _taskAssignRepository.UpdateTaskAssignAsync(existingTask);

        return (true, "Task assign updated successfully.");
    }
}
