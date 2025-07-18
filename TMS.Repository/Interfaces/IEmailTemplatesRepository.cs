using TMS.Repository.Data;

namespace TMS.Repository.Interfaces;

public interface IEmailTemplatesRepository
{
    public Task<List<EmailTemplate>> GetEmailTemplates();
    public Task<EmailTemplate?> GetEmailTemplateByName(string name);
    public Task<int> AddAsync(EmailTemplate emailTemplate);
    public Task<EmailTemplate> UpdateAsync(EmailTemplate emailTemplate);
    public Task<EmailTemplate?> GetEmailTemplateById(int id);
}
