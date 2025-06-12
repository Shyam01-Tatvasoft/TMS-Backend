using Microsoft.EntityFrameworkCore;
using TMS.Repository.Data;
using TMS.Repository.Interfaces;

namespace TMS.Repository.Implementations;

public class TaskAssignRepository : ITaskAssignRepository
{
    private readonly TmsContext _context;
    public TaskAssignRepository(TmsContext context)
    {
        _context = context;
    }

    public async Task<List<TaskAssign>> GetAllTaskAssignAsync()
    {
        return await _context.TaskAssigns
        .Include(t => t.FkTask)
        .Include(t => t.FkSubtask)
        .ToListAsync();
    }

    public async Task<TaskAssign?> GetTaskAssignAsync(int id)
    {
        return await _context.TaskAssigns.Where(t => t.Id == id)
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
