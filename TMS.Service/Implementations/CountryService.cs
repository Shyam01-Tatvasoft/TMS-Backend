using TMS.Repository.Data;
using TMS.Repository.Dtos;
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

    public async Task<List<CountryDto>> GetCountries()
    {
        List<Country> countries = await _countryRepository.GetCountries();
        List<CountryDto> countryDtos = countries.Select(c => new CountryDto
        {
            Id = c.Id,
            Name = c.Name,
            IsoCode = c.IsoCode,
            Flag = c.Flag,
            PhoneCode = c.PhoneCode
        }).ToList();
        return countryDtos;
    }

    public async Task<List<CountryTimezoneDto>> GetTimezonesByCountryId(int id)
    {
        List<CountryTimezone> timezones = await _timezoneRepository.GetTimezonesByCountryId(id);
        List<CountryTimezoneDto> countryTimezones = timezones.Select(t => new CountryTimezoneDto
        {
            Id = t.Id,
            Timezone = t.Timezone,
            FkCountryId = t.FkCountryId,
            Zone = t.Zone,
            Offset = t.Offset
        }).ToList();
        return countryTimezones;
    }

    public async Task<string> ImportCountriesAsync()
    {
        string result = await _countryRepository.ImportCountriesAsync();
        return result;
    }
}
