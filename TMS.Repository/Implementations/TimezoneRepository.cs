using Microsoft.EntityFrameworkCore;
using TMS.Repository.Data;
using TMS.Repository.Interfaces;

namespace TMS.Repository.Implementations;

public class TimezoneRepository:ITimezoneRepository
{
    private readonly TmsContext _context;

    public TimezoneRepository(TmsContext context)
    {
        _context = context;
    }

    public async Task<List<TimezoneDetail>> GetTimezonesByCountryId(int id)
    {
        List<TimezoneDetail> timezones = await _context.TimezoneDetails.Where(t => t.FkCountryId == id).ToListAsync();
        return timezones;
    }
}
