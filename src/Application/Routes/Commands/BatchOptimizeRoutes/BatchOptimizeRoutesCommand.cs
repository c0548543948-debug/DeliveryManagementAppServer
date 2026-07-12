using DeliveryManagementApp.Application.Common.Interfaces;
using DeliveryManagementApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DeliveryManagementApp.Application.Routes.Commands.BatchOptimizeRoutes;

public record BatchOptimizeRoutesCommand(DateOnly Date) : IRequest<List<int>>;

public class BatchOptimizeRoutesCommandHandler : IRequestHandler<BatchOptimizeRoutesCommand, List<int>>
{
    private readonly IApplicationDbContext _context;
    private readonly IRouteOptimizationService _optimizationService;

    public BatchOptimizeRoutesCommandHandler(IApplicationDbContext context, IRouteOptimizationService optimizationService)
    {
        _context = context;
        _optimizationService = optimizationService;
    }

    public async Task<List<int>> Handle(BatchOptimizeRoutesCommand request, CancellationToken cancellationToken)
    {
        var ordersQuery = _context.Orders
            .Include(o => o.Customer)
            .Where(o => o.Status == OrderStatus.Pending
                     && o.RequiredDate == request.Date);

        var orders = await _context.ExecuteQueryAsync(ordersQuery, cancellationToken);

        if (orders.Count == 0)
            return new List<int>();

        var allVehicles = await _context.ExecuteQueryAsync(_context.Vehicles, cancellationToken);
        if (allVehicles.Count == 0)
            return new List<int>();

        var routesOnDate = await _context.ExecuteQueryAsync(
            _context.Routes.Where(r => r.Date == request.Date),
            cancellationToken);
        var assignedCourierIds = routesOnDate.Select(r => r.CourierId).Distinct().ToHashSet();

        var allCouriers = await _context.ExecuteQueryAsync(_context.Couriers, cancellationToken);
        var availableCouriers = allCouriers.Where(c => !assignedCourierIds.Contains(c.Id)).ToList();

        if (availableCouriers.Count == 0)
            return new List<int>();

        var pairCount = Math.Min(allVehicles.Count, availableCouriers.Count);
        var availableVehicles = Enumerable.Range(0, pairCount)
            .Select(i => (Vehicle: allVehicles[i], CourierId: availableCouriers[i].Id))
            .ToList();

        var assignments = await _optimizationService.OptimizeAsync(orders, availableVehicles, request.Date, cancellationToken);

        var createdRouteIds = new List<int>();

        foreach (var assignment in assignments)
        {
            if (assignment.Stops.Count == 0)
                continue;

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
            createdRouteIds.Add(route.Id);
        }

        return createdRouteIds;
    }
}
