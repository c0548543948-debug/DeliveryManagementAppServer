using System.Text.Json.Serialization;

namespace DeliveryManagementApp.Infrastructure.Models.GoogleMaps;

// מחלקות עבור Distance Matrix
public class GoogleDistanceResponse
{
    [JsonPropertyName("rows")] public List<Row> Rows { get; set; } = new();
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
}

public class Row
{
    [JsonPropertyName("elements")] public List<Element> Elements { get; set; } = new();
}

public class Element
{
    [JsonPropertyName("distance")] public Distance Distance { get; set; } = new();
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
}

public class Distance
{
    [JsonPropertyName("value")] public int Value { get; set; }
}

// מחלקות עבור Geocoding
public class GoogleGeocodeResponse
{
    [JsonPropertyName("results")] public List<GeocodeResult> Results { get; set; } = new();
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
}

public class GeocodeResult
{
    [JsonPropertyName("partial_match")] public bool PartialMatch { get; set; }
    [JsonPropertyName("geometry")] public Geometry Geometry { get; set; } = new();
}

public class Geometry
{
    [JsonPropertyName("location")] public Location Location { get; set; } = new();
    [JsonPropertyName("location_type")] public string LocationType { get; set; } = string.Empty;
}

public class Location
{
    [JsonPropertyName("lat")] public double Lat { get; set; }
    [JsonPropertyName("lng")] public double Lng { get; set; }
}
