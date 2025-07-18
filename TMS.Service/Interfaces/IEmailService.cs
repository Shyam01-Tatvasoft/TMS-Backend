using TMS.Repository.Data;
using TMS.Repository.Dtos;

namespace TMS.Service.Interfaces;

public interface IEmailService
{
    public Task<int> SendMail(string toEmail, string subject, string body);
    public Task<EmailTemplateDto?> GetEmailTemplateById(int id);
    public Task<(bool, string)> AddEmailTemplate(EmailTemplateDto emailTemplate);
    public Task<List<EmailTemplateDto>> GetEmailTemplates();
    public Task<EmailTemplate> UpdateAsync(EmailTemplateDto emailTemplate);
    public string BuildBody(string template, Dictionary<string, string> values);
    public Task<EmailTemplateDto?> GetEmailTemplateByName(string name);
}
