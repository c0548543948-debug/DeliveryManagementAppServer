//using System.Text.Json;
//using DeliveryManagementApp.Application.Common.Interfaces;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;

//namespace DeliveryManagementApp.Infrastructure.Services;

//public class GoogleMapsService : IGoogleMapsService
//{
//    private readonly HttpClient _httpClient;
//    private readonly string _apiKey;
//    private readonly ILogger<GoogleMapsService> _logger;

//    public GoogleMapsService(HttpClient httpClient, IConfiguration configuration, ILogger<GoogleMapsService> logger)
//    {
//        _httpClient = httpClient;
//        _apiKey = configuration["GoogleMaps:ApiKey"]!;
//        _logger = logger;
//    }

//    public async Task<double?> GetDistanceInKmAsync(string origin, string destination)
//    {
//        var url = $"https://maps.googleapis.com/maps/api/distancematrix/json" +
//                  $"?origins={Uri.EscapeDataString(origin)}" +
//                  $"&destinations={Uri.EscapeDataString(destination)}" +
//                  $"&key={_apiKey}";

//        var response = await _httpClient.GetStringAsync(url);
//        _logger.LogInformation("DistanceMatrix response: {Response}", response);

//        var json = JsonDocument.Parse(response);
//        var element = json.RootElement.GetProperty("rows")[0].GetProperty("elements")[0];

//        if (element.GetProperty("status").GetString() != "OK") return null;

//        return element.GetProperty("distance").GetProperty("value").GetDouble() / 1000.0;
//    }

//    public async Task<bool> ValidateAddressAsync(string address)
//    {
//        var result = await GetGeocodeResultAsync(address);
//        if (result is null) return false;

//        if (result.Value.partialMatch) return false;

//        var acceptedTypes = new[] { "ROOFTOP", "RANGE_INTERPOLATED", "GEOMETRIC_CENTER" };
//        return acceptedTypes.Contains(result.Value.locationType);
//    }

//    public async Task<GeoCoordinates?> GetCoordinatesAsync(string address)
//    {
//        var result = await GetGeocodeResultAsync(address);
//        return result is null ? null : new GeoCoordinates(result.Value.lat, result.Value.lng);
//    }

//    private async Task<(double lat, double lng, string locationType, bool partialMatch)?> GetGeocodeResultAsync(string address)
//    {
//        var url = $"https://maps.googleapis.com/maps/api/geocode/json" +
//                  $"?address={Uri.EscapeDataString(address)}" +
//                  $"&key={_apiKey}";

//        var response = await _httpClient.GetStringAsync(url);
//        _logger.LogInformation("Geocode response for '{Address}': {Response}", address, response);

//        var json = JsonDocument.Parse(response);
//        var status = json.RootElement.GetProperty("status").GetString();
//        if (status != "OK") return null;

//        var results = json.RootElement.GetProperty("results");
//        if (results.GetArrayLength() == 0) return null;

//        var first = results[0];
//        var partialMatch = first.TryGetProperty("partial_match", out var pm) && pm.GetBoolean();
//        var locationType = first.GetProperty("geometry").GetProperty("location_type").GetString()!;
//        var location = first.GetProperty("geometry").GetProperty("location");
//        var lat = location.GetProperty("lat").GetDouble();
//        var lng = location.GetProperty("lng").GetDouble();

//        return (lat, lng, locationType, partialMatch);
//    }
//}
using System.Net.Http.Json;
using DeliveryManagementApp.Application.Common.Interfaces;
using DeliveryManagementApp.Infrastructure.Models.GoogleMaps;
using Microsoft.AspNetCore.WebUtilities; // דורש חבילת NuGet: Microsoft.AspNetCore.WebUtilities
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DeliveryManagementApp.Infrastructure.Services;

public class GoogleMapsService : IGoogleMapsService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<GoogleMapsService> _logger;

    public GoogleMapsService(HttpClient httpClient, IConfiguration configuration, ILogger<GoogleMapsService> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GoogleMaps:ApiKey"]!;
        _logger = logger;
    }

    public async Task<double?> GetDistanceInKmAsync(string origin, string destination)
    {
        var url = QueryHelpers.AddQueryString("https://maps.googleapis.com/maps/api/distancematrix/json", new Dictionary<string, string?>
        {
            { "origins", origin },
            { "destinations", destination },
            { "key", _apiKey }
        });

        var result = await _httpClient.GetFromJsonAsync<GoogleDistanceResponse>(url);

        if (result?.Status != "OK" || result.Rows.Count == 0 || result.Rows[0].Elements[0].Status != "OK")
            return null;

        return result.Rows[0].Elements[0].Distance.Value / 1000.0;
    }

    public async Task<bool> ValidateAddressAsync(string address)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            return true; // No API key configured — skip validation

        var (result, status) = await GetGeocodeResultAsync(address);

        if (status is "REQUEST_DENIED" or "UNKNOWN_ERROR")
            return true; // API key issue — skip validation gracefully

        if (result is null) return false;
        if (result.Value.partialMatch) return false;

        var acceptedTypes = new[] { "ROOFTOP", "RANGE_INTERPOLATED", "GEOMETRIC_CENTER" };
        return acceptedTypes.Contains(result.Value.locationType);
    }

    public async Task<GeoCoordinates?> GetCoordinatesAsync(string address)
    {
        var (result, _) = await GetGeocodeResultAsync(address);
        return result is null ? null : new GeoCoordinates(result.Value.lat, result.Value.lng);
    }

    private async Task<((double lat, double lng, string locationType, bool partialMatch)? result, string status)> GetGeocodeResultAsync(string address)
    {
        var url = QueryHelpers.AddQueryString("https://maps.googleapis.com/maps/api/geocode/json", new Dictionary<string, string?>
        {
            { "address", address },
            { "key", _apiKey }
        });

        var response = await _httpClient.GetFromJsonAsync<GoogleGeocodeResponse>(url);
        var status = response?.Status ?? "UNKNOWN_ERROR";

        if (status != "OK" || response!.Results.Count == 0)
            return (null, status);

        var first = response.Results[0];
        return ((
            first.Geometry.Location.Lat,
            first.Geometry.Location.Lng,
            first.Geometry.LocationType,
            first.PartialMatch
        ), status);
    }
}
