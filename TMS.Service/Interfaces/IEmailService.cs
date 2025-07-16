namespace TMS.Service.Interfaces;

public interface IEmailService
{
    public Task<int> SendMail(string toEmail, string subject, string body);
}
