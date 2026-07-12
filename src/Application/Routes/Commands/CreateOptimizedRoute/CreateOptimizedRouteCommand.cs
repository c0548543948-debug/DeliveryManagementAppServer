using DeliveryManagementApp.Application.Common.Interfaces;
using DeliveryManagementApp.Domain.Entities;
using DeliveryManagementApp.Domain.Enums;

namespace DeliveryManagementApp.Application.Routes.Commands.CreateOptimizedRoute;

/// <summary>
/// Creates a route with manual courier + vehicle selection, then optimizes the stop order.
/// Each order produces two stops: Pickup (origin) then Delivery (destination).
/// </summary>
public record CreateOptimizedRouteCommand(
    int CourierId,
    int VehicleId,
    List<int> OrderIds,
    DateOnly Date) : IRequest<int>;

public class CreateOptimizedRouteCommandHandler : IRequestHandler<CreateOptimizedRouteCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IRouteOptimizationService _optimizationService;

    public CreateOptimizedRouteCommandHandler(
        IApplicationDbContext context,
        IRouteOptimizationService optimizationService)
    {
        _context = context;
        _optimizationService = optimizationService;
    }

    public async Task<int> Handle(CreateOptimizedRouteCommand request, CancellationToken cancellationToken)
    {
        // Validate courier
        var courierExists = await _context.AnyAsync<Courier>(
            c => c.Id == request.CourierId, cancellationToken);
        Guard.Against.NotFound(request.CourierId, courierExists ? (object)true : null,
            $"Courier {request.CourierId} not found.");

        // Validate vehicle
        var vehicle = await _context.ExecuteSingleAsync(
            _context.Vehicles.Where(v => v.Id == request.VehicleId), cancellationToken);
        Guard.Against.NotFound(request.VehicleId, vehicle,
            $"Vehicle {request.VehicleId} not found.");

        // Load orders
        var ordersQuery = _context.Orders.Where(o => request.OrderIds.Contains(o.Id));
        var orders = await _context.ExecuteQueryAsync(ordersQuery, cancellationToken);

        if (orders.Count == 0)
            throw new InvalidOperationException("No orders found for the given IDs.");

        // Optimize stop order (falls back to sequential if no API key)
        var vehiclePairs = new List<(Vehicle Vehicle, int CourierId)> { (vehicle!, request.CourierId) };
        var assignments = await _optimizationService.OptimizeAsync(
            orders, vehiclePairs, request.Date, cancellationToken);

        // Build route
        var route = new Domain.Entities.Route
        {
            CourierId = request.CourierId,
            VehicleId = request.VehicleId,
            Date = request.Date,
            StartTime = TimeOnly.MinValue,
            EndTime = TimeOnly.MinValue
        };

        _context.Add(route);
        await _context.SaveChangesAsync(cancellationToken);

        var stops = (assignments.Count > 0 && assignments[0].Stops.Count > 0)
            ? assignments[0].Stops
            : orders.Select((o, i) => new RouteStop(o, new TimeOnly(8, 0).AddMinutes(i * 30))).ToList();

        foreach (var stop in stops)
        {
            var pickup = route.AddStop(stop.Order, StopType.Pickup);
            pickup.EstimatedArrival = stop.EstimatedArrival;

            var delivery = route.AddStop(stop.Order, StopType.Delivery);
            delivery.EstimatedArrival = stop.EstimatedArrival.AddMinutes(stop.Order.ServiceTimeMinutes + 1);

            _context.Add(new ActivityLog
            {
                OrderId = stop.Order.Id,
                OldStatus = stop.Order.Status,
                NewStatus = OrderStatus.Assigned,
                Timestamp = DateTime.UtcNow
            });
            stop.Order.Status = OrderStatus.Assigned;
        }

        var etas = route.Items
            .Where(i => i.EstimatedArrival.HasValue)
            .Select(i => i.EstimatedArrival!.Value)
            .OrderBy(t => t)
            .ToList();

        if (etas.Count > 0)
        {
            route.StartTime = etas.First();
            route.EndTime = etas.Last();
        }

        await _context.SaveChangesAsync(cancellationToken);
        return route.Id;
    }
}
