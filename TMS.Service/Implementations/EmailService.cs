using System.Net;
using System.Net.Mail;
using TMS.Service.Interfaces;

namespace TMS.Service.Implementations;

public class EmailService : IEmailService
{
  public int SendMail(string ToEmail, string subject, string body)
  {
    string SenderMail = "test.dotnet@etatvasoft.com";
    string SenderPassword = "P}N^{z-]7Ilp";
    string Host = "mail.etatvasoft.com";
    int Port = 587;

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
