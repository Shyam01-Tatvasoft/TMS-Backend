namespace TMS.Service.Interfaces;

public interface IHolidayService
{
    public Task<bool> IsHolidayAsync(string countryCode, DateTime date);
    public Task<List<DateTime>> GetHolidaysAsync(string isoCode, DateTime fromDate, DateTime toDate);
}
