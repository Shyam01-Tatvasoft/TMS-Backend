using Microsoft.AspNetCore.Http;

namespace TMS.Repository.Dtos;

public class UploadFileTaskDto
{
    public int FkTaskId { get; set; }
    public int FkUserId { get; set; }
    public List<IFormFile> Files { get; set; }
}
