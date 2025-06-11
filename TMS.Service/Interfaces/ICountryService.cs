using TMS.Repository.Data;

namespace TMS.Service.Interfaces;

public interface ICountryService
{
    public Task<List<Country>> GetCountries();
    public Task<List<TimezoneDetail>> GetTimezonesByCountryId(int id);
    public Task<string> ImportCountriesAsync();
}
