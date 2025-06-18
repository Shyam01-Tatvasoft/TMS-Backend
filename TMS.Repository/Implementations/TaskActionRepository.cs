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
        return await _context.TaskActions.FindAsync(id);
    }

    public async Task<TaskAction> AddTaskActionAsync(TaskAction taskAction)
    {
        _context.TaskActions.Add(taskAction);
        await _context.SaveChangesAsync();
        return taskAction;
    }
}
