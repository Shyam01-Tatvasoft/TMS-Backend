using System.Text.Json;
using TMS.Repository.Data;
using TMS.Repository.Dtos;
using TMS.Repository.Interfaces;
using TMS.Service.Enums;
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

    public async Task<List<TaskDto>> GetAllTasksAsync()
    {
        List<Repository.Data.Task> tasks = await _taskRepository.GetAllTasksAsync();
        List<TaskDto> taskDtos = tasks.Select(task => new TaskDto
        {
            Id = task.Id,
            Name = task.Name
        }).ToList();
        return taskDtos;
    }

    public async Task<List<SubTaskDto>> GetSubTasksByTaskIdAsync(int id)
    {
        List<SubTask> subTasks = await _taskRepository.GetSubTasksByTaskIdAsync(id);
        List<SubTaskDto> subTaskDtos = subTasks.Select(subTask => new SubTaskDto
        {
            Id = subTask.Id,
            Name = subTask.Name,
            FkTaskId = subTask.FkTaskId,
        }).ToList();

        return subTaskDtos;
    }

    public async Task<List<TaskAssignDto>> GetAllTaskAssignAsync(int id, string role)
    {
        List<TaskAssign> TaskAssignList = await _taskAssignRepository.GetAllTaskAssignAsync(id, role);
        List<TaskAssignDto> taskAssignDtos = TaskAssignList.Select(taskAssign => new TaskAssignDto
        {
            Id = taskAssign.Id,
            UserName = taskAssign?.FkUser?.FirstName + " " + taskAssign?.FkUser?.LastName,
            Description = taskAssign.Description,
            TaskData = !string.IsNullOrEmpty(taskAssign.TaskData) ? JsonSerializer.Deserialize<JsonElement>(taskAssign.TaskData!) : null,
            DueDate = taskAssign.DueDate,
            Status = ((Status.StatusEnum)taskAssign.Status.Value).ToString(),
            Priority = ((Priority.PriorityEnum)taskAssign.Priority.Value).ToString(),
            CreatedAt = taskAssign.CreatedAt,
            TaskName = taskAssign.FkTask?.Name ?? string.Empty,
            SubTaskName = taskAssign.FkSubtask?.Name ?? string.Empty
        }).ToList();

        return taskAssignDtos;
    }

    public async Task<UpdateTaskDto?> GetTaskAssignAsync(int id)
    {
        TaskAssign taskAssign = await _taskAssignRepository.GetTaskAssignAsync(id);
        UpdateTaskDto TaskAssignDto = new()
        {
            Id = taskAssign.Id,
            FkUserId = taskAssign.FkUserId,
            FkTaskId = taskAssign.FkTaskId,
            FkSubTaskId = taskAssign.FkSubtaskId,
            TaskData = !string.IsNullOrEmpty(taskAssign.TaskData) ? JsonSerializer.Deserialize<JsonElement>(taskAssign.TaskData!) : null,
            Status = taskAssign.Status,
            Priority = taskAssign.Priority,
            DueDate = taskAssign.DueDate,
            Description = taskAssign.Description,
        };

        return TaskAssignDto;
    }

    public async Task<(int id, string message)> AddTaskAssignAsync(AddTaskDto task)
    {
        TaskAssign newTask = new()
        {
            Description = task.Description,
            FkUserId = task.FkUserId,
            FkTaskId = task.FkTaskId,
            FkSubtaskId = task.FkSubtaskId,
            TaskData = JsonSerializer.Serialize(task.TaskData),
            DueDate = task.DueDate,
            Status = task.Status,
            Priority = task.Priority,
            CreatedAt = DateTime.Now,  
        };
        await _taskAssignRepository.AddTaskAssignAsync(newTask);

        return (newTask.Id, "Task assigned successfully.");
    }

    public async Task<(bool success, string message)> UpdateTaskAssignAsync(EditTaskDto task)
    {
        TaskAssign? existingTask = await _taskAssignRepository.GetTaskAssignAsync(task.Id);
        if (existingTask == null)
        {
            return (false, "Task not found.");
        }

        existingTask.Description = task.Description;
        existingTask.TaskData = JsonSerializer.Serialize(task.TaskData);
        existingTask.DueDate = task.DueDate;
        existingTask.Status = task.Status;
        existingTask.Priority = task.Priority;

        await _taskAssignRepository.UpdateTaskAssignAsync(existingTask);

        return (true, "Task updated successfully.");
    }
}
