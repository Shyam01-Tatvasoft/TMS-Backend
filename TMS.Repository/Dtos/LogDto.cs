using System.Text.Json;

namespace TMS.Repository.Dtos;

public class LogDto
{
    public int Id { get; set; }

    public string? FkUserId { get; set; }

    public DateTime? Date { get; set; }

    public string? Action { get; set; }

    public JsonElement? Data { get; set; }

    public string? Message { get; set; }

    public string? StackTrash { get; set; }
}
