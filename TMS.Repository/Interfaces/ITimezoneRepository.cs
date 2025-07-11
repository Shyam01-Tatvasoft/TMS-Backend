using TMS.Repository.Data;

namespace TMS.Repository.Interfaces;

public interface ITimezoneRepository
{
    public Task<List<CountryTimezone>> GetTimezonesByCountryId(int id);
}
