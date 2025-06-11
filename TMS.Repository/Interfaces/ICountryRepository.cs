using TMS.Repository.Data;

namespace TMS.Repository.Interfaces;

public interface ICountryRepository
{
    public Task<List<Country>> GetCountries();
    public Task<string> ImportCountriesAsync();
}
