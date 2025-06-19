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

    public async Task<List<CountryTimezone>> GetTimezonesByCountryId(int id)
    {
        List<CountryTimezone> timezones = await _context.CountryTimezones.Where(t => t.FkCountryId == id).OrderBy(t => t.Timezone).ToListAsync();
        return timezones;
    }
}
