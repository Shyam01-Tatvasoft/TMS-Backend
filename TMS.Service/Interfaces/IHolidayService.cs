namespace TMS.Service.Interfaces;

public interface IHolidayService
{
    public Task<bool> IsHolidayAsync(string countryCode, DateTime date);
}
