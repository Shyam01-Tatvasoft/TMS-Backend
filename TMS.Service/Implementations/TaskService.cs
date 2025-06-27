using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using TMS.Repository.Data;
using TMS.Repository.Dtos;
using TMS.Repository.Enums;
using TMS.Repository.Interfaces;
using TMS.Service.Interfaces;

namespace TMS.Service.Implementations;

public class TaskService : ITaskService
{
    private readonly ITaskAssignRepository _taskAssignRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;
    private readonly IHolidayService _holidayService;
    private readonly IUserRepository _userRepository;
    private readonly ITaskActionRepository _taskActionRepository;
    private readonly IHubContext<ReminderHub> _hubContext;

    public TaskService(ITaskAssignRepository taskAssignRepository, ITaskRepository taskRepository, IEmailService emailService, INotificationService notificationService, IHolidayService holidayService, IUserRepository userRepository, ITaskActionRepository taskActionRepository, IHubContext<ReminderHub> hubContext)
    {
        _taskAssignRepository = taskAssignRepository;
        _taskRepository = taskRepository;
        _emailService = emailService;
        _notificationService = notificationService;
        _holidayService = holidayService;
        _userRepository = userRepository;
        _taskActionRepository = taskActionRepository;
        _hubContext = hubContext;
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

    public async Task<(List<TaskAssignDto>, int count)> GetAllTaskAssignAsync(int id, string role, int skip, int take, string? search, string? sorting = null, string? sortDirection = null)
    {
        return await _taskAssignRepository.GetAllTaskAssignAsync(id, role, skip, take, search, sorting, sortDirection);
    }

    public async Task<UpdateTaskDto?> GetTaskAssignAsync(int id)
    {
        TaskAssign? taskAssign = await _taskAssignRepository.GetTaskAssignAsync(id);
        UpdateTaskDto TaskAssignDto = new();
        if (taskAssign != null)
        {
            TaskAction? taskAction = await _taskActionRepository.GetTaskActionByTaskIdAsync(taskAssign.Id);
            TaskAssignDto = new()
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
                FkTaskActionId = taskAction != null ? taskAction.Id : 0,
            };
        }

        return TaskAssignDto;
    }

    public async Task<(int id, string message)> AddTaskAssignAsync(AddTaskDto task, string role)
    {
        User? user = await _userRepository.GetByIdAsync((int)task.FkUserId!);
        if (user == null)
            return (0, "User not found.");
        if (role == "Admin" && task.Status.HasValue && (Status.StatusEnum)task?.Status.Value! != Status.StatusEnum.Pending && (Status.StatusEnum)task?.Status.Value! != Status.StatusEnum.Cancelled)
        {
            return (0, "Invalid status.");
        }

        bool isHoliday = await _holidayService.IsHolidayAsync(user?.FkCountry?.IsoCode!, task.DueDate);
        if (isHoliday)
        {
            return (0, $"Please assign task on another day because it's a public holiday for {user?.FirstName + " " + user?.LastName}.");
        }
        TaskAssign newTask = new()
        {
            Description = task.Description,
            FkUserId = task.FkUserId,
            FkTaskId = task.FkTaskId,
            FkSubtaskId = task.FkSubtaskId,
            TaskData = JsonSerializer.Serialize(task.TaskData),
            DueDate = task.DueDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
            Status = task.Status,
            Priority = task.Priority,
            CreatedAt = DateTime.Now,
        };
        await _taskAssignRepository.AddTaskAssignAsync(newTask);
        await _notificationService.AddNotification((int)task.FkUserId, newTask.Id, (int)Repository.Enums.Notification.NotificationEnum.Assigned);
        string emailBody = await GetTaskEmailBody(newTask.Id);
        _emailService.SendMail(newTask?.FkUser?.Email!, "New Task Assigned", emailBody);
        await _hubContext.Clients.All.SendAsync("ReceiveNotification", task.FkUserId, "New Task Assigned!");
        return (newTask.Id, "Task assigned successfully.");
    }

    public async Task<(bool success, string message)> UpdateTaskAssignAsync(EditTaskDto task, string role)
    {
        TaskAssign? existingTask = await _taskAssignRepository.GetTaskAssignAsync(task.Id);
        if (existingTask == null)
        {
            return (false, "Task not found.");
        }

        if (role == "Admin" && existingTask.Status.HasValue && (Status.StatusEnum)existingTask.Status.Value == Status.StatusEnum.InProgress && existingTask.Status != task.Status)
        {
            return (false, "Cannot change the status of a task that is in progress.");
        }

        if (role == "User" && task.Status.HasValue && (Status.StatusEnum)task?.Status.Value! != Status.StatusEnum.Pending && (Status.StatusEnum)task?.Status.Value! != Status.StatusEnum.InProgress && (Status.StatusEnum)task?.Status.Value! != Status.StatusEnum.OnHold)
        {
            return (false, "Invalid status.");
        }

        existingTask.Description = task.Description;
        existingTask.TaskData = JsonSerializer.Serialize(task.TaskData);
        existingTask.DueDate = task.DueDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
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
        emailBody = emailBody.Replace("{{Priority}}", ((Priority.PriorityEnum)task?.Priority.Value!).ToString() ?? "-");
        emailBody = emailBody.Replace("{{Status}}", ((Status.StatusEnum)task?.Status.Value!).ToString() ?? "-");
        emailBody = emailBody.Replace("{{DueDate}}", task.DueDate.ToString("dd MMM yyyy"));
        emailBody = emailBody.Replace("{{Description}}", task.Description ?? "-");

        return emailBody;
    }

    public async Task<TaskAssign?> ApproveTask(int id)
    {
        TaskAssign? taskAssign = await _taskAssignRepository.GetTaskAssignAsync(id);
        if (taskAssign == null)
        {
            return null;
        }

        taskAssign.Status = (int)Status.StatusEnum.Completed;
        await _taskAssignRepository.UpdateTaskAssignAsync(taskAssign);
        await _notificationService.AddNotification((int)taskAssign.FkUserId!, taskAssign.Id, (int)Repository.Enums.Notification.NotificationEnum.Approved);
        string emailBody = await GetTaskEmailBody(taskAssign.Id);
        _emailService.SendMail(taskAssign.FkUser?.Email!, "Task Approved", emailBody);
        await _hubContext.Clients.All.SendAsync("ReceiveNotification", (int)taskAssign.FkUserId!, "Your Task is Approved !");
        return taskAssign;
    }

    public async Task<TaskAssign> ReassignTask(ReassignTaskDto dto)
    {
        TaskAssign? taskAssign = await _taskAssignRepository.GetTaskAssignAsync(dto.TaskId);
        if (taskAssign == null)
        {
            throw new Exception("Task not found.");
        }

        taskAssign.Description = dto.Comments;
        taskAssign.Status = (int)Status.StatusEnum.Pending;

        await _taskAssignRepository.UpdateTaskAssignAsync(taskAssign);

        // Reassign task email
        string emailBody = await GetTaskEmailBody(taskAssign.Id, "TaskReassignedTemplate");
        _emailService.SendMail(taskAssign.FkUser?.Email!, "Task Reassigned", emailBody);

        // Notify user
        await _notificationService.AddNotification((int)taskAssign.FkUserId!, taskAssign.Id, (int)Repository.Enums.Notification.NotificationEnum.Reassigned);
        await _hubContext.Clients.All.SendAsync("ReceiveNotification", (int)taskAssign.FkUserId!, "Your Task is Reassigned !");
        return taskAssign;
    }

    public async Task<List<TaskAssignDto>> GetTasksForSchedular(DateTime start, DateTime end, string role, int userId)
    {
        return await _taskAssignRepository.GetTasksForSchedular(start, end, role, userId);
    }
}
