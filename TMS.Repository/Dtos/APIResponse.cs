using System.Net;

namespace TMS.Repository.Dtos;

public class APIResponse
{
    public HttpStatusCode StatusCode { get; set; }
    public bool IsSuccess { get; set; } = true;
    public List<String> ErrorMessage { get; set; }
    public object Result { get; set; }
}
