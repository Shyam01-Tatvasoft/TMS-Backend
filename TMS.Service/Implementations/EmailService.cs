using System.Net;
using System.Net.Mail;
using TMS.Repository.Data;
using TMS.Repository.Interfaces;
using TMS.Service.Constants;
using TMS.Service.Interfaces;

namespace TMS.Service.Implementations;

public class EmailService : IEmailService
{

  private readonly ISystemConfigurationRepository _systemConfigurationRepository;

  public EmailService(ISystemConfigurationRepository systemConfigurationRepository)
  {
    _systemConfigurationRepository = systemConfigurationRepository;
  }

  public async Task<int> SendMail(string ToEmail, string subject, string body)
  {
    string SenderMail = (await _systemConfigurationRepository.GetConfigByNameAsync(SystemConfigs.SenderEmail))!;
    string SenderPassword = (await _systemConfigurationRepository.GetConfigByNameAsync(SystemConfigs.SenderPassword))!;
    string Host = (await _systemConfigurationRepository.GetConfigByNameAsync(SystemConfigs.EmailHost))!;
    int Port = int.Parse((await _systemConfigurationRepository.GetConfigByNameAsync(SystemConfigs.Port))!);

    var smtpClient = new SmtpClient(Host)
    {
      Port = Port,
      Credentials = new NetworkCredential(SenderMail, SenderPassword),
    };

    MailMessage mailMessage = new MailMessage();
    mailMessage.From = new MailAddress(SenderMail);
    mailMessage.To.Add(ToEmail);
    mailMessage.Subject = subject;
    mailMessage.IsBodyHtml = true;
    mailMessage.Body = body;

    smtpClient.Send(mailMessage);
    return 1;
  }
}
