using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using System.Transactions;
using Microsoft.AspNetCore.Diagnostics;
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

    public async Task<(List<TaskAssignDto>, int count)> GetAllTaskAssignAsync(int id, string role, string? taskType, int skip, int take, string? search, string? sorting = null, string? sortDirection = null)
    {
        return await _taskAssignRepository.GetAllTaskAssignAsync(id, role, skip, take, search, sorting, sortDirection, taskType);
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
                IsRecurrence = taskAssign.IsRecurrence,
                RecurrencePattern = taskAssign.RecurrencePattern != null ? ((Recurrence.RecurrenceEnum)taskAssign.RecurrencePattern).ToDescription() : null,
                RecurrenceOn = taskAssign.RecurrenceOn,
                RecurrenceTo = taskAssign.RecurrenceTo != null ? taskAssign.RecurrenceTo : null,
                EndAfter = taskAssign.EndAfter
            };
        }

        return TaskAssignDto;
    }

    public async Task<(int id, string message)> AddTaskAssignAsync(AddTaskDto task, string role)
    {
        using TransactionScope transaction = new(TransactionScopeAsyncFlowOption.Enabled);
        try
        {
            User? user = await _userRepository.GetByIdAsync((int)task.FkUserId!);
            if (user == null)
                return (0, "User not found.");
            if (role == "Admin" && task.Status.HasValue && (Status.StatusEnum)task?.Status.Value! != Status.StatusEnum.Pending && (Status.StatusEnum)task?.Status.Value! != Status.StatusEnum.Cancelled)
            {
                return (0, "Invalid status.");
            }

            TaskAssign newTask = new()
            {
                Description = task.Description,
                FkUserId = task.FkUserId,
                FkTaskId = task.FkTaskId,
                FkSubtaskId = task.FkSubtaskId,
                TaskData = JsonSerializer.Serialize(task.TaskData),
                DueDate = DateTime.SpecifyKind(task.DueDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59), DateTimeKind.Local),
                Status = task.Status,
                Priority = task.Priority,
                CreatedAt = DateTime.Now,
            };

            if (task.IsRecurrence)
            {
                var (id, message) = await HandleRecurrenceTaskAsync(newTask, task, user?.FkCountry?.IsoCode!);
                transaction.Complete();
                return (id, message);
            }
            else
            {
                bool isHoliday = await _holidayService.IsHolidayAsync(user?.FkCountry?.IsoCode!, task.DueDate);
                if (isHoliday)
                {
                    return (0, $"Please assign task on another day because it's a public holiday for {user?.FirstName + " " + user?.LastName}.");
                }
                newTask.RecurrenceTo = null;
                await _taskAssignRepository.AddTaskAssignAsync(newTask);
                await _notificationService.AddNotification((int)task.FkUserId, newTask.Id, (int)Repository.Enums.Notification.NotificationEnum.Assigned);
                string emailBody = await GetTaskEmailBody(newTask.Id);
                _emailService.SendMail(newTask?.FkUser?.Email!, "New Task Assigned", emailBody);
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", task.FkUserId, "New Task Assigned!");
                transaction.Complete();
                return (newTask.Id, "Task assigned successfully.");
            }
        }
        catch (System.Exception)
        {
            transaction.Dispose();
            throw;
        }
    }

    private async Task<(int, string)> HandleRecurrenceTaskAsync(TaskAssign newTask, AddTaskDto task, string isoCode)
    {
        if (task.RecurrencePattern < 1 || task.RecurrencePattern > 3)
            return (0, "Invalid recurrence pattern.");

        if (task.RecurrencePattern == (int)Recurrence.RecurrenceEnum.Monthly && (task.RecurrenceOn < 1 || task.RecurrenceOn > 31))
            return (0, "Invalid recurrence day.");

        if (task.RecurrencePattern == (int)Recurrence.RecurrenceEnum.Weekly && (task.RecurrenceOn < 1 || task.RecurrenceOn > 7))
            return (0, "Invalid recurrence day of the week.");

        if (task.RecurrencePattern == (int)Recurrence.RecurrenceEnum.Daily && task.RecurrenceOn != 1)
            return (0, "Invalid recurrence day.");

        if (task.EndAfter.HasValue && (task.EndAfter.Value < 1 || task.EndAfter.Value > 100))
            return (0, "End after must be greater than 0 and less than 100.");

        //Generate due dates and start dates
        var (startDates, dueDates) = GenerateDueDates(task);

        // Fetch all holidays in a single time
        var holidays = await _holidayService.GetHolidaysAsync(isoCode, dueDates.Min(), dueDates.Max());

        if (holidays.Count > 0)
        {
            foreach (var date in dueDates)
            {
                if (holidays.Contains(date.Date))
                    return (0, $"Holiday found on {date.Date.ToShortDateString()}. Recurrence halted.");
            }
        }

        //Generate uniq guid id for recurrence tasks
        string guid = Guid.NewGuid().ToString("N");
        for (int i = 0; i < dueDates.Count; i++)
        {
            newTask.Id = 0;
            newTask.IsRecurrence = task.IsRecurrence;
            newTask.RecurrencePattern = task.RecurrencePattern;
            newTask.RecurrenceOn = task.RecurrenceOn;
            newTask.EndAfter = task.EndAfter;
            newTask.RecurrenceTo = DateTime.SpecifyKind(task.RecurrenceTo.Date, DateTimeKind.Local);
            newTask.CreatedAt = startDates[i].Date;
            newTask.DueDate = dueDates[i].Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            newTask.RecurrenceId = guid;
            await _taskAssignRepository.AddTaskAssignAsync(newTask);
        }

        return (newTask.Id, "Task assigned successfully with recurrence.");
    }


    private static (List<DateTime>, List<DateTime>) GenerateDueDates(AddTaskDto task)
    {
        List<DateTime> startDates = new();
        List<DateTime> dueDates = new();
        DateTime startDate = DateTime.SpecifyKind(task.RecurrenceTo.Date, DateTimeKind.Local);
        int count = task.EndAfter ?? 1;

        switch ((Recurrence.RecurrenceEnum)(task.RecurrencePattern ?? 1))
        {
            case Recurrence.RecurrenceEnum.Daily:
                for (int i = 0; i < count;)
                {
                    if (!IsWeekend(startDate))
                    {
                        startDates.Add(startDate);
                        if (!IsWeekend(startDate.AddDays(1)))
                            dueDates.Add(startDate.AddDays(1));
                        else
                            dueDates.Add(startDate.AddDays(3));

                        i++;
                    }
                    startDate = startDate.AddDays(1);
                }
                break;

            case Recurrence.RecurrenceEnum.Weekly:
                startDate = FindNextRecurrenceInWeek(startDate, task.RecurrenceOn);
                for (int i = 0; i < count; i++)
                {
                    startDates.Add(startDate);
                    startDate = startDate.AddDays(7);
                    dueDates.Add(startDate);
                }
                break;

            case Recurrence.RecurrenceEnum.Monthly:
                if (startDate.Day >= task.RecurrenceOn)
                {
                    startDate = startDate.AddMonths(1);
                }
                startDate = new DateTime(startDate.Year, startDate.Month, (int)task.RecurrenceOn!);

                for (int i = 0; i < count; i++)
                {
                    startDates.Add(startDate);
                    startDate = FindNextRecurrenceMonth(startDate, task.RecurrenceOn);
                    dueDates.Add(startDate);
                }
                break;
        }

        return (startDates, dueDates);
    }

    private static bool IsWeekend(DateTime Date)
    {
        return Date.DayOfWeek == DayOfWeek.Saturday || Date.DayOfWeek == DayOfWeek.Sunday;
    }

    private static DateTime FindNextRecurrenceInWeek(DateTime recurrenceTo, int? recurrenceOn)
    {
        DateTime nextRecurrence = recurrenceTo;

        while (true)
        {
            nextRecurrence = nextRecurrence.AddDays(1);
            if ((int)nextRecurrence.DayOfWeek == recurrenceOn)
            {
                break;
            }
        }
        return nextRecurrence;
    }

    private static DateTime FindNextRecurrenceMonth(DateTime recurrenceTo, int? recurrenceOn)
    {
        DateTime nextRecurrence = recurrenceTo.AddMonths(1);
        int? day = recurrenceOn;

        int daysInMonth = DateTime.DaysInMonth(nextRecurrence.Year, nextRecurrence.Month);
        if (day > daysInMonth)
        {
            day = daysInMonth;
        }

        nextRecurrence = new DateTime(nextRecurrence.Year, nextRecurrence.Month, (int)day!);
        return nextRecurrence;
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

        if (existingTask.IsRecurrence == true && existingTask.EndAfter != task.EndAfter)
        {
            return await HandleRecurrentTaskUpdate(existingTask, task);
        }

        await _taskAssignRepository.UpdateTaskAssignAsync(existingTask);

        return (true, "Task updated successfully.");
    }

    public async Task<(bool, string)> HandleRecurrentTaskUpdate(TaskAssign existingTask, EditTaskDto updatedTask)
    {
        string message;
        bool isSuccess;

        if (existingTask.EndAfter < updatedTask.EndAfter)
        {
            int newTasks = (int)updatedTask.EndAfter - (int)existingTask.EndAfter;
            existingTask.EndAfter = updatedTask.EndAfter;
            for (int i = 0; i < newTasks; i++)
            {
                await _taskAssignRepository.AddTaskAssignAsync(existingTask);
            }
            message = "Task updated successfully.";
            isSuccess = true;
        }
        else
        {
            // remove task which are not performed
            List<TaskAssign> taskAssigns = await _taskAssignRepository.GetRecurrenceTaskAsync(existingTask.RecurrenceId!);
            int count = 0;
            foreach (var task in taskAssigns)
            {
                if (task.Status != (int)Status.StatusEnum.Pending && task.CreatedAt > DateTime.Now)
                {
                    count++;
                }
            }
            if (count > updatedTask.EndAfter)
            {
                message = "You can not decrease end after less then" + count + "because they are under process.";
                isSuccess = false;
            }
            else
            {
                foreach (var task in taskAssigns)
                {
                    if (task.Status == (int)Status.StatusEnum.Pending)
                    {
                        existingTask.Id = task.Id;
                        existingTask.EndAfter = updatedTask.EndAfter;
                        existingTask.IsDeleted = true;
                        await _taskAssignRepository.UpdateTaskAssignAsync(existingTask);
                    }
                }
                message = "Task Updated Successfully.";
                isSuccess = true;
            }
        }
        return (isSuccess, message);
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
        using TransactionScope transaction = new(TransactionScopeAsyncFlowOption.Enabled);
        try
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

            transaction.Complete();
            return taskAssign;
        }
        catch (System.Exception)
        {
            transaction.Dispose();
            throw;
        }
    }

    public async Task<TaskAssign> ReassignTask(ReassignTaskDto dto)
    {
        using TransactionScope transaction = new(TransactionScopeAsyncFlowOption.Enabled);
        try
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
            transaction.Complete();
            return taskAssign;
        }
        catch (System.Exception)
        {
            transaction.Dispose();
            throw;
        }
    }

    public async Task<List<TaskAssignDto>> GetTasksForSchedular(DateTime start, DateTime end, string role, int userId)
    {
        return await _taskAssignRepository.GetTasksForSchedular(start, end, role, userId);
    }

    public async Task<List<TaskGraphDto>> GetTaskChartData(TaskGraphFilterDto filter)
    {
        return await _taskAssignRepository.GetTaskChartDataAsync(filter);
    }
}
