using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TMS.Repository.Data;
using TMS.Repository.Dtos;
using TMS.Repository.Interfaces;
using TMS.Service.Enums;

namespace TMS.Repository.Implementations;

public class TaskAssignRepository : ITaskAssignRepository
{
    private readonly TmsContext _context;
    public TaskAssignRepository(TmsContext context)
    {
        _context = context;
    }

    public async Task<(List<TaskAssignDto>,int count)> GetAllTaskAssignAsync(int id, string role, int skip, int take, string? search, string? sorting, string? sortDirection)
    {
        var query = _context.TaskAssigns
            .Include(t => t.FkUser)
            .Include(t => t.FkTask)
            .Include(t => t.FkSubtask)
            .AsQueryable();

        if (role.ToLower() != "admin")
        {
            query = query.Where(t => t.FkUser.Id == id);
        }
        if (!string.IsNullOrEmpty(sorting) && !string.IsNullOrEmpty(sortDirection))
        {
            if (sortDirection.ToLower() == "asc")
            {
                query = sorting switch
                {
                    "2" => query.OrderBy(t => t.FkUser.FirstName).ThenBy(t => t.FkUser.LastName),
                    "4" => query.OrderBy(t => t.DueDate),
                    _ => query.OrderBy(t => t.DueDate)
                };
            }
            else
            {
                query = sorting switch
                {
                    "2" => query.OrderByDescending(t => t.FkUser.FirstName).ThenByDescending(t => t.FkUser.LastName),
                    "4" => query.OrderByDescending(t => t.DueDate),
                    _ => query.OrderByDescending(t => t.DueDate)
                };
            }
        }
        else
        {
            query = query.OrderByDescending(t => t.DueDate);
        }
        if (!string.IsNullOrEmpty(search))
        {
            search = search.ToLower();
            query = query.Where(t =>
                t.FkTask.Name.ToLower().Contains(search) ||
                t.Description.Contains(search) ||
                t.FkUser.FirstName.ToLower().Contains(search) ||
                t.FkUser.LastName.ToLower().Contains(search));
        }

        int totalCount = await query.CountAsync();
        List<TaskAssignDto> tasks = await query
            .Skip(skip)
            .Take(take)
            .Select(taskAssign => new TaskAssignDto
            {
                Id = taskAssign.Id,
                UserName = taskAssign.FkUser.FirstName + " " + taskAssign.FkUser.LastName,
                Description = taskAssign.Description,
                // TaskData = !string.IsNullOrEmpty(taskAssign.TaskData) ? JsonSerializer.Deserialize<JsonElement>(taskAssign.TaskData, new JsonSerializerOptions()) : null,
                DueDate = taskAssign.DueDate,
                Status = ((Status.StatusEnum)taskAssign.Status.Value).ToString(),
                Priority =((Priority.PriorityEnum)taskAssign.Priority.Value).ToString(),
                CreatedAt = taskAssign.CreatedAt,
                TaskName = taskAssign.FkTask.Name ?? string.Empty,
                SubTaskName = taskAssign.FkSubtask.Name ?? string.Empty
            })
            .ToListAsync();

            return  (tasks, totalCount);
        // if (role == "Admin")
        // {
        //     return await _context.TaskAssigns
        //     .Include(t => t.FkUser)
        //     .Include(t => t.FkTask)
        //     .Include(t => t.FkSubtask)
        //     .ToListAsync();
        // }
        // else
        // {
        //     return await _context.TaskAssigns
        //     .Where(t => t.FkUserId == id)
        //     .Include(t => t.FkUser)
        //     .Include(t => t.FkTask)
        //     .Include(t => t.FkSubtask)
        //     .ToListAsync();
        // }
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
}
