using System.Data.Common;

namespace TMS.Repository.Dtos;

public class EmailTemplateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}
