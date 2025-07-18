using Microsoft.EntityFrameworkCore;
using TMS.Repository.Data;
using TMS.Repository.Interfaces;

namespace TMS.Repository.Implementations;

public class EmailTemplatesRepository: IEmailTemplatesRepository
{
    private readonly TmsContext _context;
    public EmailTemplatesRepository(TmsContext context)
    {
        _context = context;
    }

    public async Task<List<EmailTemplate>> GetEmailTemplates()
    {
        return await _context.EmailTemplates.ToListAsync();
    }

    public async Task<EmailTemplate?> GetEmailTemplateByName(string name)
    {
        return await _context.EmailTemplates.FirstOrDefaultAsync(et => et.Name == name);
    }

    public async Task<EmailTemplate?> GetEmailTemplateById(int id)
    {
        return await _context.EmailTemplates.FirstOrDefaultAsync(et => et.Id == id);
    }
    
    public async Task<int> AddAsync(EmailTemplate emailTemplate)
    {
        _context.EmailTemplates.Add(emailTemplate);
        return await _context.SaveChangesAsync();
    }
    
    public async Task<EmailTemplate> UpdateAsync(EmailTemplate emailTemplate)
    {
        _context.EmailTemplates.Update(emailTemplate);
        await _context.SaveChangesAsync();
        return emailTemplate;
    }
}
