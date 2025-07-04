using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TMS.Repository.Data;
using TMS.Repository.Dtos;
using TMS.Repository.Enums;
using TMS.Repository.Interfaces;

namespace TMS.Repository.Implementations;

public class TaskAssignRepository : ITaskAssignRepository
{
    private readonly TmsContext _context;
    public TaskAssignRepository(TmsContext context)
    {
        _context = context;
    }

    public async Task<(List<TaskAssignDto>, int count)> GetAllTaskAssignAsync(int id, string role, int skip, int take, string? search, string? sorting, string? sortDirection, string? taskType, int? statusFilter, int? userFilter)
    {
        var query = _context.TaskAssigns.Where(t => t.IsDeleted == false)
            .Include(t => t.FkUser)
            .Include(t => t.FkTask)
            .Include(t => t.FkSubtask)
            .AsQueryable();

        if (role.ToLower() != "admin")
        {
            query = query.Where(t => t.FkUser.Id == id);
        }

        if (role.ToLower() == "admin")
        {
            if (taskType == "recurrence")
                query = query.Where(t => t.IsRecurrence == true);
            else
                query = query.Where(t => t.IsRecurrence == false);

            if (userFilter != 0 && userFilter != null)
            {
                query = query.Where(t => t.FkUserId == userFilter);
            }
        }
        if (statusFilter != 0 && statusFilter != null)
        {
            query = query.Where(t => t.Status == statusFilter);
        }

        if (!string.IsNullOrEmpty(sorting) && !string.IsNullOrEmpty(sortDirection))
        {
            if (sortDirection.ToLower() == "asc")
            {
                query = sorting switch
                {
                    "2" => query.OrderBy(t => t.FkUser.FirstName).ThenBy(t => t.FkUser.LastName),
                    "4" => query.OrderBy(t => t.DueDate),
                    "5" => query.OrderBy(t => t.Status.ToString()),
                    _ => query.OrderBy(t => t.Id)
                };
            }
            else
            {
                query = sorting switch
                {
                    "2" => query.OrderByDescending(t => t.FkUser.FirstName).ThenByDescending(t => t.FkUser.LastName),
                    "4" => query.OrderByDescending(t => t.DueDate),
                    "5" => query.OrderBy(t => t.Status.ToString()),
                    _ => query.OrderByDescending(t => t.Id)
                };
            }
        }
        else
        {
            query = query.OrderByDescending(t => t.Id);
        }
        if (!string.IsNullOrEmpty(search))
        {
            search = search.ToLower();
            query = query.Where(t =>
                t.FkTask.Name.ToLower().Contains(search) ||
                t.Description.Contains(search) ||
                (t.FkUser.FirstName + " " + t.FkUser.LastName).ToLower().Contains(search) ||
                t.FkUser.FirstName.ToLower().Contains(search) ||
                t.FkUser.LastName.ToLower().Contains(search));
        }

        int totalCount;
        // if(role.ToLower() == "admin" && taskType == "recurrence")
        // {
        //     query =  query.GroupBy(t => t.RecurrenceId)
        //     .Select(g => g.FirstOrDefault());
        //     totalCount = await query.CountAsync();
        //     List<TaskAssign> taskList = await query.Skip(skip).Take(take).ToListAsync();
        // }

        totalCount = await query.CountAsync();

        List<TaskAssignDto> tasks = await query
            .Skip(skip)
            .Take(take)
            .Select(taskAssign => new TaskAssignDto
            {
                Id = taskAssign.Id,
                UserName = taskAssign.FkUser.FirstName + " " + taskAssign.FkUser.LastName,
                Description = taskAssign.Description,
                DueDate = taskAssign.DueDate,
                Status = ((Status.StatusEnum)taskAssign.Status).ToDescription(),
                Priority = ((Priority.PriorityEnum)taskAssign.Priority.Value).ToString(),
                CreatedAt = taskAssign.CreatedAt,
                TaskName = taskAssign.FkTask.Name ?? string.Empty,
                SubTaskName = taskAssign.FkSubtask.Name ?? string.Empty,
                IsRecurrent = taskAssign.IsRecurrence,
                RecurrenceId = taskAssign.RecurrenceId,
                TaskActionId = ((Status.StatusEnum?)taskAssign.Status == Status.StatusEnum.Review ||
                (Status.StatusEnum)taskAssign.Status == Status.StatusEnum.Completed)
                ? _context.TaskActions
                    .Where(ta => ta.FkTaskId == taskAssign.Id)
                    .Select(ta => ta.Id)
                    .FirstOrDefault() : 0,
            })
            .ToListAsync();

        if (role.ToLower() == "admin" && taskType == "recurrence")
        {
            var groupedTasks = tasks.GroupBy(t => t.RecurrenceId)
                .Select(g => g.First()).ToList();

            tasks = groupedTasks;
        }


        return (tasks, totalCount);
    }

    public async Task<TaskAssign?> GetTaskAssignAsync(int id)
    {
        return await _context.TaskAssigns.Where(t => t.Id == id)
        .Include(t => t.FkUser)
        .Include(t => t.FkTask)
        .ThenInclude(t => t.SubTasks)
        .FirstOrDefaultAsync();
    }

    public async System.Threading.Tasks.Task AddTaskAssignAsync(TaskAssign task)
    {
        await _context.TaskAssigns.AddAsync(task);
        await _context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task UpdateTaskAssignAsync(TaskAssign task)
    {
        _context.TaskAssigns.Update(task);
        await _context.SaveChangesAsync();
    }

    public async Task<List<TaskAssignDto>> GetTasksForSchedular(DateTime start, DateTime end, string role, int userId)
    {
        var startUnspecified = DateTime.SpecifyKind(start.Date, DateTimeKind.Unspecified);
        var endUnspecified = DateTime.SpecifyKind(end.Date, DateTimeKind.Unspecified);

        List<TaskAssignDto> taskList = await _context.TaskAssigns
            .Where(t => (role == "Admin") || (role == "User" && t.FkUserId == userId))
            .Where(t => t.DueDate.Date > startUnspecified && t.DueDate.Date < endUnspecified && t.IsDeleted == false)
            .Include(t => t.FkUser)
            .Include(t => t.FkTask)
            .Include(t => t.FkSubtask)
            .Select(taskAssign => new TaskAssignDto
            {
                Id = taskAssign.Id,
                UserName = taskAssign.FkUser.Username,
                Description = taskAssign.Description,
                DueDate = taskAssign.DueDate,
                Status = ((Status.StatusEnum)taskAssign.Status).ToDescription(),
                Priority = ((Priority.PriorityEnum)taskAssign.Priority.Value).ToString(),
                CreatedAt = taskAssign.CreatedAt,
                CompletedAt = _context.TaskActions.FirstOrDefault(a => a.FkTaskId == taskAssign.Id)!.SubmittedAt,
                TaskName = taskAssign.FkTask.Name ?? string.Empty,
                SubTaskName = taskAssign.FkSubtask.Name ?? string.Empty
            })
            .ToListAsync();

        return taskList;
    }

    public async Task<List<TaskAssign>> GetDueTasksAsync()
    {
        var tomorrow = DateTime.Today.AddDays(1);

        List<TaskAssign> taskList = await _context.TaskAssigns.Where(t => t.DueDate.Date == tomorrow &&
                t.Status != (int)Status.StatusEnum.Completed && t.Status != (int)Status.StatusEnum.Review && t.Status != (int)Status.StatusEnum.Cancelled)
            .Include(t => t.FkUser)
            .Include(t => t.TaskActions)
            .ToListAsync();

        return taskList;
    }

    public async Task<List<TaskAssign>> GetOverdueTasksAsync()
    {
        var today = DateTime.Today;

        List<TaskAssign> taskList = await _context.TaskAssigns.Where(t => t.DueDate.Date < today &&
                t.Status != (int)Status.StatusEnum.Completed && t.Status != (int)Status.StatusEnum.Review && t.Status != (int)Status.StatusEnum.Cancelled)
            .Include(t => t.FkUser)
            .Include(t => t.TaskActions)
            .ToListAsync();

        return taskList;
    }

    public async Task<List<TaskAssign>> GetTodaysRecurrentTasksAsync()
    {
        var today = DateTime.Today;
        List<TaskAssign> taskList = await _context.TaskAssigns.Where(t => t.IsRecurrence == true && t.CreatedAt.HasValue && t.CreatedAt.Value.Date == today.Date)
            .Include(t => t.FkUser)
            .ToListAsync();

        return taskList;
    }

    public async Task<List<TaskGraphDto>> GetTaskChartDataAsync(TaskGraphFilterDto filter)
    {
        var filteredTasks = _context.TaskAssigns.AsQueryable();
        int filterStatus = (int)(filter.Status == null ? (int)Status.StatusEnum.InProgress : filter.Status);
        DateTime? fromDate = DateTime.SpecifyKind(filter.FromDate.Value, DateTimeKind.Local);
        DateTime? toDate = DateTime.SpecifyKind(filter.ToDate.Value, DateTimeKind.Local);


        if (fromDate.HasValue && toDate.HasValue)
        {
            filteredTasks = filteredTasks.Where(t => t.CreatedAt >= fromDate && t.CreatedAt <= toDate);
        }

        if (filter.Status != 0)
            filteredTasks = filteredTasks.Where(t => t.Status == filter.Status);

        var taskChartData = await filteredTasks
            .GroupBy(t => t.FkUserId)
            .Select(g => new TaskGraphDto
            {
                UserName = g.FirstOrDefault().FkUser.Username,
                TaskCount = g.Count(),
                Status = g.FirstOrDefault().Status.HasValue ? ((Status.StatusEnum)g.FirstOrDefault().Status!).ToDescription() : "null"
            }).ToListAsync();
        return taskChartData;
    }

    public async Task<List<TaskAssign>> GetRecurrenceTaskAsync(string recurrenceId)
    {
        return await _context.TaskAssigns.Where(t => t.RecurrenceId == recurrenceId && t.IsDeleted == false).OrderByDescending(t => t.DueDate).ToListAsync();
    }
}
