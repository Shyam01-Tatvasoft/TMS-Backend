using Microsoft.EntityFrameworkCore;
using TMS.Repository.Data;
using TMS.Repository.Interfaces;

namespace TMS.Repository.Implementations;

public class TaskActionRepository : ITaskActionRepository
{
    private readonly TmsContext _context;

    public TaskActionRepository(TmsContext context)
    {
        _context = context;
    }

    public async Task<List<TaskAction>> GetAllTaskActionsAsync()
    {
        return await _context.TaskActions.ToListAsync();
    }

    public async Task<TaskAction?> GetTaskActionByIdAsync(int id)
    {
        return await _context.TaskActions.Where(t => t.Id == id)
            .Include(t => t.FkTask)
            .Include(t => t.FkTask.FkTask)
            .Include(t => t.FkTask.FkSubtask)
            .Include(t => t.FkUser)
            .FirstOrDefaultAsync();
    }

    public async Task<TaskAction> AddTaskActionAsync(TaskAction taskAction)
    {
        _context.TaskActions.Add(taskAction);
        await _context.SaveChangesAsync();
        return taskAction;
    }

    public async Task<TaskAction?> GetTaskActionByTaskIdAsync(int taskId)
    {
        return await _context.TaskActions
            .Where(t => t.FkTaskId == taskId)
            .Include(t => t.FkUser)
            .FirstOrDefaultAsync();
    }

    public async Task<int> UpdateTaskActionAsync(TaskAction taskAction)
    {
        _context.TaskActions.Update(taskAction);
        return await _context.SaveChangesAsync();
    }
}
