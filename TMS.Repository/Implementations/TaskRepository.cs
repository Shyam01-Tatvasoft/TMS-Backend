using Microsoft.EntityFrameworkCore;
using TMS.Repository.Data;
using TMS.Repository.Interfaces;

namespace TMS.Repository.Implementations;

public class TaskRepository : ITaskRepository
{
     private readonly TmsContext _context;
    public TaskRepository(TmsContext context)
    {
        _context = context;
    }

    public async Task<List<TMS.Repository.Data.Task>> GetAllTasksAsync()
    {
        return await _context.Tasks.ToListAsync();
    }

    public async Task<List<TMS.Repository.Data.SubTask>> GetSubTasksByTaskIdAsync(int id)
    {
        return await _context.SubTasks.Where(w => w.FkTaskId == id).ToListAsync();
    }
}
