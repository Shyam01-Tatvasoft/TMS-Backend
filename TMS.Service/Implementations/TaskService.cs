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
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;

    public TaskService(ITaskAssignRepository taskAssignRepository, ITaskRepository taskRepository,IEmailService emailService,INotificationService notificationService)
    {
        _taskAssignRepository = taskAssignRepository;
        _taskRepository = taskRepository;
        _emailService = emailService;
        _notificationService = notificationService;
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

    public async Task<(List<TaskAssignDto>,int count)> GetAllTaskAssignAsync(int id, string role, int skip, int take, string? search, string? sorting = null, string? sortDirection = null)
    {
        return await _taskAssignRepository.GetAllTaskAssignAsync(id, role,skip,take,search, sorting, sortDirection);
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
        await _notificationService.AddNotification((int)task.FkUserId, newTask.Id);
        string emailBody = await GetTaskEmailBody(newTask.Id);
        _emailService.SendMail(newTask.FkUser.Email, "New Task Assigned", emailBody);
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

    public async Task<string> GetTaskEmailBody(int id, string templateName = "TaskEmailTemplate")
    {
        TaskAssign? task = await _taskAssignRepository.GetTaskAssignAsync(id);
        string templatePath = $"d:/TMS/TMS.Service/Templates/{templateName}.html";

        if (!System.IO.File.Exists(templatePath))
        {
            return "<p>Email template not found</p>";
        }

        string emailBody = System.IO.File.ReadAllText(templatePath);

        emailBody = emailBody.Replace("{{UserName}}", task?.FkUser.FirstName + " " + task.FkUser.LastName ?? "User");
        emailBody = emailBody.Replace("{{TaskType}}", task.FkTask?.Name ?? "-");
        emailBody = emailBody.Replace("{{SubTask}}", task.FkSubtask?.Name ?? "-");
        emailBody = emailBody.Replace("{{Priority}}", ((Priority.PriorityEnum)task?.Priority.Value).ToString() ?? "-");
        emailBody = emailBody.Replace("{{Status}}", ((Status.StatusEnum)task?.Status.Value).ToString() ?? "-");
        emailBody = emailBody.Replace("{{DueDate}}", task.DueDate.ToString("dd MMM yyyy"));
        emailBody = emailBody.Replace("{{Description}}", task.Description ?? "-");

        return emailBody;
    }
}
