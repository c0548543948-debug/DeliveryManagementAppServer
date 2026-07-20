using System.Collections.Concurrent;
using DeliveryManagementApp.Application.Common.Interfaces;

namespace DeliveryManagementApp.Infrastructure.Services;

/// <summary>
/// Tracks courier route progress in memory (resets on server restart).
/// Stores current stop number per route.
/// </summary>
public class RouteProgressService : IRouteProgressService
{
    private record RouteProgress(int CurrentStop, int TotalStops);
    private readonly ConcurrentDictionary<int, RouteProgress> _progress = new();

    public void StartRoute(int routeId, int totalStops)
        => _progress[routeId] = new RouteProgress(1, totalStops);

    public void SetCurrentStop(int routeId, int stopNumber)
        => _progress.AddOrUpdate(
            routeId,
            new RouteProgress(stopNumber, stopNumber),
            (_, existing) => new RouteProgress(stopNumber, existing.TotalStops));

    public (int CurrentStop, int TotalStops)? GetProgress(int routeId)
        => _progress.TryGetValue(routeId, out var p) ? (p.CurrentStop, p.TotalStops) : null;
}
