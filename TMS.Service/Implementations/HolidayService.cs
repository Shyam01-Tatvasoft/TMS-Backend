using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;
using TMS.Service.Interfaces;

namespace TMS.Service.Implementations;

public class HolidayService : IHolidayService
{
    private readonly HttpClient _httpClient;

    public HolidayService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> IsHolidayAsync(string countryCode, DateTime date)
    {
        if (string.IsNullOrEmpty(countryCode)) return false;
        string year = date.Year.ToString();
        string url = $"https://date.nager.at/api/v3/PublicHolidays/{year}/{countryCode}";

        try
        {
            var response = await _httpClient.GetAsync(url);
            if(response.ReasonPhrase == "No Content")
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

    private class HolidayDto
    {
        public DateTime Date { get; set; }
    }
}
