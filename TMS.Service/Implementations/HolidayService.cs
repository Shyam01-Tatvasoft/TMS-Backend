using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;
using TMS.Repository.Interfaces;
using TMS.Service.Constants;
using TMS.Service.Interfaces;

namespace TMS.Service.Implementations;

public class HolidayService : IHolidayService
{
    private readonly HttpClient _httpClient;
    private readonly ISystemConfigurationRepository _systemConfigurationRepository;
    public HolidayService(HttpClient httpClient,ISystemConfigurationRepository systemConfigurationRepository)
    {
        _httpClient = httpClient;
        _systemConfigurationRepository = systemConfigurationRepository;
    }

    public async Task<bool> IsHolidayAsync(string countryCode, DateTime date)
    {
        if (string.IsNullOrEmpty(countryCode)) return false;
        string year = date.Year.ToString();
        string holidayUrl = (await _systemConfigurationRepository.GetConfigByNameAsync(SystemConfigs.HolidayApi))!;
        string url = holidayUrl + year + "/" + countryCode;

        try
        {
            var response = await _httpClient.GetAsync(url);
            if (response.ReasonPhrase == "No Content")
            {
                return false;
            }
            if (!response.IsSuccessStatusCode) return false;

            var holidays = await response.Content.ReadFromJsonAsync<List<HolidayDto>>();
            return holidays?.Any(h => h.Date == date.Date) ?? true;
        }
        catch (System.Exception ex)
        {
            return false;
        }
    }

    public async Task<List<DateTime>> GetHolidaysAsync(string isoCode, DateTime fromDate, DateTime toDate)
    {
        using var httpClient = new HttpClient();
        List<DateTime> allHolidays = new();

        for (int year = fromDate.Year; year <= toDate.Year; year++)
        {
            // var response = await httpClient.GetFromJsonAsync<List<HolidayDto>>(
            //     $"https://date.nager.at/api/v3/PublicHolidays/{year}/{isoCode}"
            // );

            var response = await _httpClient.GetAsync($"https://date.nager.at/api/v3/PublicHolidays/{year}/{isoCode}");
            if (response.ReasonPhrase == "No Content")
            {
                return allHolidays;
            }
            if (!response.IsSuccessStatusCode) return allHolidays;

            var holidays = await response.Content.ReadFromJsonAsync<List<HolidayDto>>();

            if (response != null)
            {
                allHolidays.AddRange(
                    holidays
                        .Where(h => h.Date.Date >= fromDate.Date && h.Date.Date <= toDate.Date)
                        .Select(h => h.Date.Date)
                );
            }
        }

        return allHolidays.Distinct().ToList();
    }

    private class HolidayDto
    {
        public DateTime Date { get; set; }
    }
}
