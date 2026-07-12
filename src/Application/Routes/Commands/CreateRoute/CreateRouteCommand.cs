using DeliveryManagementApp.Application.Common.Interfaces;

namespace DeliveryManagementApp.Application.Routes.Commands.CreateRoute;

public record CreateRouteCommand(
    int CourierId,
    int? VehicleId,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime) : IRequest<int>;

public class CreateRouteCommandHandler : IRequestHandler<CreateRouteCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateRouteCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<int> Handle(CreateRouteCommand request, CancellationToken cancellationToken)
    {
            var courierExists = await _context.AnyAsync<DeliveryManagementApp.Domain.Entities.Courier>(c => c.Id == request.CourierId, cancellationToken);
        Guard.Against.NotFound(request.CourierId, courierExists ? (object)true : null,
            $"Courier with ID {request.CourierId} not found.");

        if (request.VehicleId.HasValue)
        {
            var vehicleExists = await _context.AnyAsync<DeliveryManagementApp.Domain.Entities.Vehicle>(v => v.Id == request.VehicleId, cancellationToken);
            Guard.Against.NotFound(request.VehicleId.Value, vehicleExists ? (object)true : null,
                $"Vehicle with ID {request.VehicleId} not found.");
        }

        var route = new Domain.Entities.Route
        {
            CourierId = request.CourierId,
            VehicleId = request.VehicleId,
            Date = request.Date,
            StartTime = request.StartTime,
            EndTime = request.EndTime
        };

        _context.Add<DeliveryManagementApp.Domain.Entities.Route>(route);
        await _context.SaveChangesAsync(cancellationToken);
        return route.Id;
    }
}
