using TMS.Repository.Data;
using TMS.Repository.Dtos;

namespace TMS.Service.Interfaces;

public interface ICountryService
{
    public Task<List<CountryDto>> GetCountries();
    public Task<List<CountryTimezoneDto>> GetTimezonesByCountryId(int id);
    public Task<string> ImportCountriesAsync();
}
