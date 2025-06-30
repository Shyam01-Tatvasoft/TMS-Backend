namespace TMS.Service.Interfaces;

public interface ITaskReminderService
{
    public Task DueDateReminderService();
    public Task OverdueReminderService();
}
