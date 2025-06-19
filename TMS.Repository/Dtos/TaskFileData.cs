namespace TMS.Repository.Dtos;

public class TaskFileData
{
    public string OriginalFileName { get; set; }
    public string StoredFileName { get; set; }
    public long Size { get; set; }
    public DateTime UploadedAt { get; set; }
}
