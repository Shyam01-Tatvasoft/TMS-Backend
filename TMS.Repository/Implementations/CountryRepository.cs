using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TMS.Repository.Data;
using TMS.Repository.Dtos;
using TMS.Repository.Interfaces;

namespace TMS.Repository.Implementations;

public class CountryRepository: ICountryRepository
{
    private readonly TmsContext _context;
     private readonly HttpClient _httpClient;

    public CountryRepository(TmsContext context,HttpClient httpClient)
    {
        _context = context;
        _httpClient = httpClient;
    }

    public async Task<List<Country>> GetCountries()
    {
        List<Country> countries = await _context.Countries
            // .Include(c => c.Timezones)
            .ToListAsync();
        return countries;
    }

    public async Task<string> ImportCountriesAsync()
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("https://country-state-city-search-rest-api.p.rapidapi.com/allcountries"),
            Headers =
            {
                { "X-RapidAPI-Key", "04c359e481msh193f2089d017a37p159b72jsnd109f04920fc" },
                { "X-RapidAPI-Host", "country-state-city-search-rest-api.p.rapidapi.com" }
            }
        };

        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var jsonString = await response.Content.ReadAsStringAsync();

        var apiCountries = JsonConvert.DeserializeObject<List<CountryDto>>(jsonString);

        foreach (var dto in apiCountries!)
        {
            if (_context.Countries.Any(c => c.IsoCode == dto.IsoCode))
                continue;

            var country = new Country
            {
                Name = dto.Name,
                IsoCode = dto.IsoCode,
                Flag = dto.Flag,
                PhoneCode = dto.PhoneCode,
                TimezoneDetails = dto.timezones?.Select(tz => new TimezoneDetail
                {
                    Timezone = tz.tzName,
                    Zone = tz.zoneName,
                    Offset = tz.gmtOffsetName
                }).ToList() ?? new List<TimezoneDetail>()
            };

            _context.Countries.Add(country);
        }

        await _context.SaveChangesAsync();
        return "Country and timezone data imported successfully.";
    }
}
