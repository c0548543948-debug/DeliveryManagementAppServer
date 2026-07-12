using DeliveryManagementApp.Application.Common.Interfaces;
using DeliveryManagementApp.Domain.Entities;
using DeliveryManagementApp.Domain.Enums;

namespace DeliveryManagementApp.Application.Routes.Commands.OptimizeRoutes;

public record OptimizeRoutesCommand(DateOnly Date) : IRequest<List<int>>;

public class OptimizeRoutesCommandHandler : IRequestHandler<OptimizeRoutesCommand, List<int>>
{
    private readonly IApplicationDbContext _context;
    private readonly IRouteOptimizationService _optimizationService;

    public OptimizeRoutesCommandHandler(IApplicationDbContext context, IRouteOptimizationService optimizationService)
    {
        _context = context;
        _optimizationService = optimizationService;
    }

    public async Task<List<int>> Handle(OptimizeRoutesCommand request, CancellationToken cancellationToken)
    {
        // Include all orders that are not delivered, cancelled, or already in transit for routing
        var ordersQuery = _context.Orders
            .Where(o => o.Status != OrderStatus.Delivered
                     && o.Status != OrderStatus.Cancelled
                     && o.Status != OrderStatus.InTransit
                     && o.RequiredDate == request.Date);

        var orders = await _context.ExecuteQueryAsync(ordersQuery, cancellationToken);

        if (orders.Count == 0) return new List<int>();

        // Load all vehicles
        var allVehicles = await _context.ExecuteQueryAsync(_context.Vehicles, cancellationToken);

        // Find couriers already assigned a route on the requested date
        var routesOnDate = await _context.ExecuteQueryAsync(_context.Routes.Where(r => r.Date == request.Date), cancellationToken);
        var assignedCourierIds = routesOnDate.Select(r => r.CourierId).Distinct().ToHashSet();

        // Find couriers who are free on that date
        var allCouriers = await _context.ExecuteQueryAsync(_context.Couriers, cancellationToken);
        var availableCouriers = allCouriers.Where(c => !assignedCourierIds.Contains(c.Id)).ToList();
        if (availableCouriers.Count == 0) return new List<int>();

        // Pair vehicles to available couriers up to the smaller of the two counts
        var pairCount = Math.Min(allVehicles.Count, availableCouriers.Count);
        var availableVehicles = Enumerable.Range(0, pairCount)
            .Select(i => (Vehicle: allVehicles[i], CourierId: availableCouriers[i].Id))
            .ToList();

        var assignments = await _optimizationService.OptimizeAsync(orders, availableVehicles, request.Date, cancellationToken);

        var createdRouteIds = new List<int>();

        foreach (var assignment in assignments)
        {
            if (assignment.Stops.Count == 0) continue;

            var route = new Domain.Entities.Route
            {
                CourierId = assignment.CourierId,
                VehicleId = assignment.VehicleId,
                Date = request.Date,
                // StartTime/EndTime will be calculated after stops are added
                StartTime = TimeOnly.MinValue,
                EndTime = TimeOnly.MinValue
            };

            _context.Add(route);
            await _context.SaveChangesAsync(cancellationToken);

            // For each assigned order create a pickup stop then a delivery stop (pickup before delivery)
            foreach (var stop in assignment.Stops)
            {
                // Add pickup
                var pickupItem = route.AddStop(stop.Order, StopType.Pickup);
                pickupItem.EstimatedArrival = stop.EstimatedArrival;

                // Add delivery after pickup
                var deliveryItem = route.AddStop(stop.Order, StopType.Delivery);
                deliveryItem.EstimatedArrival = stop.EstimatedArrival.AddMinutes(stop.Order.ServiceTimeMinutes + 1);

                // Mark order as assigned
                _context.Add(new ActivityLog
                {
                    OrderId = stop.Order.Id,
                    OldStatus = stop.Order.Status,
                    NewStatus = OrderStatus.Assigned,
                    Timestamp = DateTime.UtcNow
                });
                stop.Order.Status = OrderStatus.Assigned;
            }

            // Calculate route StartTime and EndTime based on ETA of created items
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
            createdRouteIds.Add(route.Id);
        }

        return createdRouteIds;
    }
}
