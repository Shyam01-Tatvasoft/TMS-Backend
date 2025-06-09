using Microsoft.EntityFrameworkCore;
using TMS.Repository.Data;
using TMS.Repository.Interfaces;

namespace TMS.Repository.Implementations;

public class CountryRepository: ICountryRepository
{
    private readonly TmsContext _context;

    public CountryRepository(TmsContext context)
    {
        _context = context;
    }

    public async Task<List<Country>> GetCountries()
    {
        List<Country> countries = await _context.Countries
            // .Include(c => c.Timezones)
            .ToListAsync();
        return countries;
    }
}
