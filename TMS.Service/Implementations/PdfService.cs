using iText.Html2pdf;
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

public class PdfService: IPdfService
{
    private readonly IRazorViewEngine _viewEngine;
    private readonly ITempDataProvider _tempDataProvider;
    private readonly IServiceProvider _serviceProvider;
    private readonly ITaskAssignRepository _taskAssignRepository;

    public PdfService(IRazorViewEngine viewEngine, ITempDataProvider tempDataProvider, IServiceProvider serviceProvider, ITaskAssignRepository taskAssignRepository)
    {
        _viewEngine = viewEngine;
        _tempDataProvider = tempDataProvider;
        _serviceProvider = serviceProvider;
        _taskAssignRepository = taskAssignRepository;
    }

    public async Task<byte[]> GenerateTaskReportPdfAsync(TaskFilterDto taskFilterDto)
    {
        List<TaskAssignDto> tasks = await _taskAssignRepository.GetFilteredTaskForPdfAsync(taskFilterDto);
        var htmlContent = await RenderViewAsync("Templates/TaskPdf.cshtml", tasks);

        using var ms = new MemoryStream();
        HtmlConverter.ConvertToPdf(htmlContent, ms);
        return ms.ToArray();
    }

    private async Task<string> RenderViewAsync<TaskAssignDto>(string viewPath, TaskAssignDto model)
    {
        var actionContext = new ActionContext(
            new DefaultHttpContext { RequestServices = _serviceProvider },
            new Microsoft.AspNetCore.Routing.RouteData(),
            new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()
        );

        var viewResult = _viewEngine.GetView(null, viewPath, true);
        if (!viewResult.Success)
            throw new InvalidOperationException($"Couldn't find view at path {viewPath}");

        var viewDictionary = new ViewDataDictionary<TaskAssignDto>(
            new EmptyModelMetadataProvider(),
            new ModelStateDictionary()
        )
        {
            Model = model
        };

        using var sw = new StringWriter();
        var viewContext = new ViewContext(
            actionContext,
            viewResult.View,
            viewDictionary,
            new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
            sw,
            new HtmlHelperOptions()
        );

        await viewResult.View.RenderAsync(viewContext);
        return sw.ToString();
    }

}
