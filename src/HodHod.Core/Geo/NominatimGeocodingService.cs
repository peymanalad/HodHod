using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace HodHod.Geo;

public class NominatimGeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient = new();

    public async Task<LocationResult> ReverseGeocodeAsync(double latitude, double longitude)
    {
        var url =
            $"https://api-map.hodhod-app.ir//reverse?format=jsonv2&lat={latitude}&lon={longitude}&zoom=10&addressdetails=1&accept-language=fa";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("User-Agent", "HodHod-App");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        if (!doc.RootElement.TryGetProperty("address", out var address))
        {
            return new LocationResult();
        }

        //string city = null;
        //if (address.TryGetProperty("city", out var cityProp))
        //{
        //    city = cityProp.GetString();
        //}
        //else if (address.TryGetProperty("town", out var townProp))
        //{
        //    city = townProp.GetString();
        //}
        //else if (address.TryGetProperty("village", out var villageProp))
        //{
        //    city = villageProp.GetString();
        //}
        string city = GetAddressValue(address, "city", "town", "village", "county");

        //string province = null;
        //if (address.TryGetProperty("state", out var stateProp))
        //{
        //    province = stateProp.GetString();
        //}
        string province = GetAddressValue(address, "state", "province", "region", "state_district");

        return new LocationResult
        {
            City = city,
            Province = province
        };

    }
    private static string GetAddressValue(JsonElement address, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (address.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.String)
            {
                var str = value.GetString();
                if (!string.IsNullOrWhiteSpace(str))
                {
                    return str;
                }
            }
        }

        return null;
    }
}