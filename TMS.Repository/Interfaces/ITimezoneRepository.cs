using TMS.Repository.Data;

namespace TMS.Repository.Interfaces;

public interface ITimezoneRepository
{
    public Task<List<TimezoneDetail>> GetTimezonesByCountryId(int id);
}
