using iText.Html2pdf;
using iText.Kernel.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using RazorLight;
using TMS.Repository.Data;
using TMS.Repository.Dtos;
using TMS.Repository.Interfaces;
using TMS.Service.Interfaces;

namespace TMS.Service.Implementations;

public class PdfService : IPdfService
{
    private readonly IRazorViewEngine _viewEngine;
    private readonly ITempDataProvider _tempDataProvider;
    private readonly IServiceProvider _serviceProvider;
    private readonly ITaskAssignRepository _taskAssignRepository;
    private readonly RazorLightEngine _razorEngine;
    public PdfService(IRazorViewEngine viewEngine, ITempDataProvider tempDataProvider, IServiceProvider serviceProvider, ITaskAssignRepository taskAssignRepository)
    {
        _viewEngine = viewEngine;
        _tempDataProvider = tempDataProvider;
        _serviceProvider = serviceProvider;
        _taskAssignRepository = taskAssignRepository;
        var templatePath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())?.FullName!, "TMS.Service", "Templates");
        _razorEngine = new RazorLightEngineBuilder()
           .UseFileSystemProject(templatePath)
           .UseMemoryCachingProvider()
           .Build();
    }
    public async Task<byte[]> GenerateTaskReportPdfAsync(TaskFilterDto taskFilterDto, string role, int userId)
    {
        if(role == "User")
        {
            taskFilterDto.UserFilter = userId;
        }
        List<TaskAssignDto> tasks = await _taskAssignRepository.GetFilteredTaskForPdfAsync(taskFilterDto);
        if (tasks == null || !tasks.Any())
            return null!;
        string htmlContent = await _razorEngine.CompileRenderAsync("TaskPdf.cshtml", tasks);

        using var ms = new MemoryStream();
        try
        {
            HtmlConverter.ConvertToPdf(htmlContent, ms);
        }
        catch (PdfException ex)
        {
            Console.WriteLine("PDF Exception: " + ex.Message);
            throw; // rethrow if needed
        }
        return ms.ToArray();
    }

}
