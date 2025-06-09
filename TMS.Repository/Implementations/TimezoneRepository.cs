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

    public async Task<List<Timezone>> GetTimezonesByCountryId(int id)
    {
        List<Timezone> timezones = await _context.Timezones.Where(t => t.CountryId == id).ToListAsync();
        return timezones;
    }
}
