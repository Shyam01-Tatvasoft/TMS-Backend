using TMS.Repository.Dtos;

namespace TMS.Service.Interfaces;

public interface IPdfService
{
    public Task<byte[]> GenerateTaskReportPdfAsync(TaskFilterDto taskFilterDto, string role, int userId);
}
