using DeliveryManagementApp.Application.Common.Interfaces;
using DeliveryManagementApp.Domain.Entities;
using DeliveryManagementApp.Domain.Enums;

namespace DeliveryManagementApp.Application.Routes.Commands.OptimizeRoute;

public record OptimizeRouteCommand(List<int> OrderIds, DateOnly Date) : IRequest<int>;

public class OptimizeRouteCommandHandler : IRequestHandler<OptimizeRouteCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IRouteOptimizationService _optimizationService;

    public OptimizeRouteCommandHandler(IApplicationDbContext context, IRouteOptimizationService optimizationService)
    {
        _context = context;
        _optimizationService = optimizationService;
    }

    public async Task<int> Handle(OptimizeRouteCommand request, CancellationToken cancellationToken)
    {
        var ordersQuery = _context.Orders
            .Where(o => request.OrderIds.Contains(o.Id)
                     && o.RequiredDate == request.Date);

        var orders = await _context.ExecuteQueryAsync(ordersQuery, cancellationToken);

        if (orders.Count == 0)
            throw new InvalidOperationException("No orders found for the specified date. Orders must have RequiredDate matching the route date.");

        var allVehicles = await _context.ExecuteQueryAsync(_context.Vehicles, cancellationToken);
        if (allVehicles.Count == 0)
            throw new InvalidOperationException("No vehicles available for route optimization.");

        var routesOnDate = await _context.ExecuteQueryAsync(
            _context.Routes.Where(r => r.Date == request.Date),
            cancellationToken);
        var assignedCourierIds = routesOnDate.Select(r => r.CourierId).Distinct().ToHashSet();

        var allCouriers = await _context.ExecuteQueryAsync(_context.Couriers, cancellationToken);
        var availableCouriers = allCouriers.Where(c => !assignedCourierIds.Contains(c.Id)).ToList();

        if (availableCouriers.Count == 0)
            throw new InvalidOperationException("No available couriers for the specified date.");

        var pairCount = Math.Min(allVehicles.Count, availableCouriers.Count);
        var availableVehicles = Enumerable.Range(0, pairCount)
            .Select(i => (Vehicle: allVehicles[i], CourierId: availableCouriers[i].Id))
            .ToList();

        var assignments = await _optimizationService.OptimizeAsync(orders, availableVehicles, request.Date, cancellationToken);

        if (assignments.Count == 0 || assignments[0].Stops.Count == 0)
            throw new InvalidOperationException("Optimization produced no valid route.");

        var assignment = assignments[0];

        var route = new Domain.Entities.Route
        {
            CourierId = assignment.CourierId,
            VehicleId = assignment.VehicleId,
            Date = request.Date,
            StartTime = TimeOnly.MinValue,
            EndTime = TimeOnly.MinValue
        };

        _context.Add(route);
        await _context.SaveChangesAsync(cancellationToken);

        foreach (var stop in assignment.Stops)
        {
            var pickupItem = route.AddStop(stop.Order, StopType.Pickup);
            pickupItem.EstimatedArrival = stop.EstimatedArrival;

            var deliveryItem = route.AddStop(stop.Order, StopType.Delivery);
            deliveryItem.EstimatedArrival = stop.EstimatedArrival.AddMinutes(stop.Order.ServiceTimeMinutes + 1);

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
