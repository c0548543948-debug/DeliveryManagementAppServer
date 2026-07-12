using DeliveryManagementApp.Application.Common.Interfaces;
using DeliveryManagementApp.Domain.Enums;

namespace DeliveryManagementApp.Application.Routes.Commands.ReoptimizeRoute;

public record ReoptimizeRouteCommand(int RouteId) : IRequest<int>;

public class ReoptimizeRouteCommandHandler : IRequestHandler<ReoptimizeRouteCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IRouteOptimizationService _optimizationService;

    public ReoptimizeRouteCommandHandler(IApplicationDbContext context, IRouteOptimizationService optimizationService)
    {
        _context = context;
        _optimizationService = optimizationService;
    }

    public async Task<int> Handle(ReoptimizeRouteCommand request, CancellationToken cancellationToken)
    {
        // Load the existing route with items
        var route = await _context.GetRouteWithItemsAsync(request.RouteId, cancellationToken);
        if (route is null)
            throw new InvalidOperationException($"Route with ID {request.RouteId} not found.");

        // Extract orders from existing route items
        var orders = route.Items
            .Select(item => item.Order)
            .Distinct()
            .ToList();

        if (orders.Count == 0)
            throw new InvalidOperationException("Route has no orders to reoptimize.");

        // Get the vehicle and courier for this route
        var vehicle = route.Vehicle;
        if (vehicle is null)
            throw new InvalidOperationException("Route has no assigned vehicle.");

        var availableVehicles = new List<(Domain.Entities.Vehicle Vehicle, int CourierId)>
        {
            (Vehicle: vehicle, CourierId: route.CourierId)
        };

        // Call optimization service to recalculate the route
        var assignments = await _optimizationService.OptimizeAsync(orders, availableVehicles, route.Date, cancellationToken);

        if (assignments.Count == 0 || assignments[0].Stops.Count == 0)
            throw new InvalidOperationException("Reoptimization produced no valid route.");

        var assignment = assignments[0];

        // Clear existing route items
        foreach (var item in route.Items.ToList())
        {
            route.Items.Remove(item);
        }

        // Add new optimized stops
        foreach (var stop in assignment.Stops)
        {
            // Add pickup
            var pickupItem = route.AddStop(stop.Order, StopType.Pickup);
            pickupItem.EstimatedArrival = stop.EstimatedArrival;

            // Add delivery after pickup
            var deliveryItem = route.AddStop(stop.Order, StopType.Delivery);
            deliveryItem.EstimatedArrival = stop.EstimatedArrival.AddMinutes(stop.Order.ServiceTimeMinutes + 1);
        }

        // Recalculate route times based on new ETAs
        var etas = route.Items
            .Where(i => i.EstimatedArrival.HasValue)
            .Select(i => i.EstimatedArrival!.Value)
            .OrderBy(d => d)
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
