using TMS.Repository.Data;
using TMS.Repository.Interfaces;
using TMS.Service.Interfaces;

namespace TMS.Service.Implementations;

public class CountryService : ICountryService
{
    private readonly ICountryRepository _countryRepository;
    private readonly ITimezoneRepository _timezoneRepository;

    public CountryService(ICountryRepository countryRepository,ITimezoneRepository timezoneRepository)
    {
        _countryRepository = countryRepository;
        _timezoneRepository = timezoneRepository;
    }

    public async Task<List<Country>> GetCountries()
    {
        List<Country> countries = await _countryRepository.GetCountries();
        return countries;
    }

    public async Task<List<Timezone>> GetTimezonesByCountryId(int id)
    {
        List<Timezone> timezones = await _timezoneRepository.GetTimezonesByCountryId(id);
        return timezones;
    }
}
