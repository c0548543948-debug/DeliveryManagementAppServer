using DeliveryManagementApp.Domain.Entities;

namespace DeliveryManagementApp.Application.Common.Interfaces;

public record RouteStop(Order Order, TimeOnly EstimatedArrival);
public record VehicleAssignment(int VehicleId, int CourierId, List<RouteStop> Stops);

public interface IRouteOptimizationService
{
    Task<List<VehicleAssignment>> OptimizeAsync(
        List<Order> orders,
        List<(Vehicle Vehicle, int CourierId)> availableVehicles,
        DateOnly date,
        CancellationToken cancellationToken = default);
}
