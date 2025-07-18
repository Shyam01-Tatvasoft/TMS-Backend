using System.Net;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using TMS.Repository.Data;
using TMS.Repository.Dtos;
using TMS.Repository.Interfaces;
using TMS.Service.Constants;
using TMS.Service.Interfaces;

namespace TMS.Service.Implementations;

public class EmailService : IEmailService
{

  private readonly ISystemConfigurationRepository _systemConfigurationRepository;
  private readonly IEmailTemplatesRepository _emailTemplatesRepository;
  public EmailService(ISystemConfigurationRepository systemConfigurationRepository, IEmailTemplatesRepository emailTemplatesRepository)
  {
    _emailTemplatesRepository = emailTemplatesRepository;
    _systemConfigurationRepository = systemConfigurationRepository;
  }

  public async Task<List<EmailTemplateDto>> GetEmailTemplates()
  {
    var emailTemplates = await _emailTemplatesRepository.GetEmailTemplates();
    return emailTemplates.Select(et => new EmailTemplateDto
    {
      Id = et.Id,
      Name = et.Name,
      Body = et.Body
    }).ToList();
  }

  public async Task<(bool, string)> AddEmailTemplate(EmailTemplateDto emailTemplate)
  {
    var existingTemplate = await _emailTemplatesRepository.GetEmailTemplateByName(emailTemplate.Name);
    if (existingTemplate != null)
    {
      return (false, $"Email template with name {emailTemplate.Name} already exists.");
    }

    EmailTemplate newEmailTemplate = new EmailTemplate
    {
      Name = emailTemplate.Name,
      Body = emailTemplate.Body
    };
    await _emailTemplatesRepository.AddAsync(newEmailTemplate);
    return (true, $"Email template with name {emailTemplate.Name} added successfully.");
  }

  public async Task<EmailTemplateDto?> GetEmailTemplateByName(string name)
  {
    var emailTemplate = await _emailTemplatesRepository.GetEmailTemplateByName(name);
    if (emailTemplate == null)
    {
      return null;
    }
    EmailTemplateDto template = new()
    {
      Id = emailTemplate.Id,
      Name = emailTemplate.Name.Trim(),
      Body = emailTemplate.Body,
    };
    return template;
  }

  public async Task<EmailTemplateDto?> GetEmailTemplateById(int id)
  {
    var emailTemplate = await _emailTemplatesRepository.GetEmailTemplateById(id);
    if (emailTemplate == null)
    {
      return null;
    }
    EmailTemplateDto template = new()
    {
      Id = emailTemplate.Id,
      Name = emailTemplate.Name,
      Body = emailTemplate.Body,
    };
    return template;
  }

  public async Task<EmailTemplate?> UpdateAsync(EmailTemplateDto emailTemplate)
  {
    var existingTemplate = await _emailTemplatesRepository.GetEmailTemplateById(emailTemplate.Id);
    if (existingTemplate == null)
    {
      return null!;
    }

    existingTemplate.Body = emailTemplate.Body;
    existingTemplate.Name = emailTemplate.Name;
    return await _emailTemplatesRepository.UpdateAsync(existingTemplate);
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


  public string BuildBody(string template, Dictionary<string, string> values)
  {
    foreach (var pair in values)
    {
      template = template.Replace("{{" + pair.Key + "}}", pair.Value);
    }
    return template;
  }
}
