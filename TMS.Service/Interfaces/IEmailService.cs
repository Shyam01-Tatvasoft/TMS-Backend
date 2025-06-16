namespace TMS.Service.Interfaces;

public interface IEmailService
{
    public int SendMail(string toEmail, string subject, string body);
}
