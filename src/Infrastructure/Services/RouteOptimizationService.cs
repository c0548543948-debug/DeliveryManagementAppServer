using System.Text;
using System.Text.Json;
using DeliveryManagementApp.Application.Common.Interfaces;
using DeliveryManagementApp.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DeliveryManagementApp.Infrastructure.Services;

public class RouteOptimizationService : IRouteOptimizationService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<RouteOptimizationService> _logger;

    public RouteOptimizationService(HttpClient httpClient, IConfiguration configuration, ILogger<RouteOptimizationService> logger)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
        _apiKey = configuration["GoogleMaps:ApiKey"] ?? string.Empty;
        _logger = logger;
    }

    public async Task<List<VehicleAssignment>> OptimizeAsync(
        List<Order> orders,
        List<(Vehicle Vehicle, int CourierId)> availableVehicles,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var depotTime = new TimeOnly(8, 0);

        // No API key configured — use local fallback immediately
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            _logger.LogWarning("GoogleMaps:ApiKey not configured. Using fallback route ordering.");
            return FallbackAssignment(orders, availableVehicles, depotTime);
        }

        var shipments = orders.Select(o => new
        {
            pickups = new[]
            {
                new
                {
                    arrivalLocation = new { address = new { addressLines = new[] { o.OriginAddress } } },
                    duration = $"{o.ServiceTimeMinutes}s"
                }
            },
            deliveries = new[]
            {
                new
                {
                    arrivalLocation = new { address = new { addressLines = new[] { o.DestinationAddress } } },
                    duration = $"{o.ServiceTimeMinutes}s"
                }
            },
            loadDemands = new
            {
                weight = new { amount = (long)(o.Weight * 100) },
                volume = new { amount = (long)(o.Volume * 100) }
            }
        }).ToList<object>();

        var vehicles = availableVehicles.Select(v => new
        {
            startLocation = new { address = new { addressLines = new[] { "Tel Aviv, Israel" } } },
            endLocation   = new { address = new { addressLines = new[] { "Tel Aviv, Israel" } } },
            loadLimits = new
            {
                weight = new { maxLoad = (long)(v.Vehicle.CapacityWeight * 100) },
                volume = new { maxLoad = (long)(v.Vehicle.CapacityVolume * 100) }
            },
            costPerHour = 50,
            costPerKilometer = 2
        }).ToList<object>();

        var globalStartDateTime = new DateTime(date.Year, date.Month, date.Day, depotTime.Hour, depotTime.Minute, 0, DateTimeKind.Utc);
        var globalEndDateTime = globalStartDateTime.AddHours(10);

        var payload = new
        {
            model = new
            {
                shipments,
                vehicles,
                globalStartTime = globalStartDateTime.ToString("o"),
                globalEndTime   = globalEndDateTime.ToString("o")
            }
        };

        try
        {
            var url = $"https://routeoptimization.googleapis.com/v1/projects/-:optimizeTours?key={_apiKey}";
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogInformation("RouteOptimization response: {Response}", responseBody);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("RouteOptimization failed, using fallback: {Body}", responseBody);
                return FallbackAssignment(orders, availableVehicles, depotTime);
            }

            var assignments = ParseResponse(responseBody, orders, availableVehicles, depotTime);
            if (assignments == null || assignments.Count == 0)
            {
                _logger.LogWarning("RouteOptimization returned no assignments, using fallback.");
                return FallbackAssignment(orders, availableVehicles, depotTime);
            }

            return assignments;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RouteOptimization request failed, using fallback.");
            return FallbackAssignment(orders, availableVehicles, depotTime);
        }
    }

    private static List<VehicleAssignment> ParseResponse(
        string responseBody,
        List<Order> orders,
        List<(Vehicle Vehicle, int CourierId)> vehicles,
        TimeOnly depotTime)
    {
        var json = JsonDocument.Parse(responseBody);
        var assignments = new List<VehicleAssignment>();

        // Try several possible locations for the route list returned by the API
        JsonElement routes;
        if (!json.RootElement.TryGetProperty("routes", out routes))
        {
            if (json.RootElement.TryGetProperty("tours", out routes))
            {
                // ok
            }
            else if (json.RootElement.TryGetProperty("solution", out var solution) && solution.TryGetProperty("routes", out routes))
            {
                // ok
            }
            else
            {
                return FallbackAssignment(orders, vehicles, depotTime);
            }
        }

        foreach (var route in routes.EnumerateArray())
        {
            var vehicleIndex = route.GetProperty("vehicleIndex").GetInt32();
            var vehicle = vehicles[vehicleIndex];
            var stops = new List<RouteStop>();

            if (!route.TryGetProperty("visits", out var visits)) continue;

            foreach (var visit in visits.EnumerateArray())
            {
                var shipmentIndex = visit.GetProperty("shipmentIndex").GetInt32();
                var order = orders[shipmentIndex];
                var eta = visit.TryGetProperty("startTime", out var st)
                    ? TimeOnly.Parse(st.GetString()!)
                    : depotTime;

                stops.Add(new RouteStop(order, eta));
            }

            assignments.Add(new VehicleAssignment(vehicle.Vehicle.Id, vehicle.CourierId, stops));
        }

        return assignments;
    }

    private static List<VehicleAssignment> FallbackAssignment(
        List<Order> orders,
        List<(Vehicle Vehicle, int CourierId)> vehicles,
        TimeOnly depotTime)
    {
        var assignments = vehicles
            .Select(v => new VehicleAssignment(v.Vehicle.Id, v.CourierId, new List<RouteStop>()))
            .ToList();

        for (var i = 0; i < orders.Count; i++)
            assignments[i % assignments.Count].Stops.Add(new RouteStop(orders[i], depotTime.AddMinutes(i * 30)));

        return assignments;
    }
}
