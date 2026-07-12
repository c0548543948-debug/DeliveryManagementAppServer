using DeliveryManagementApp.Application.Common.Interfaces;

namespace DeliveryManagementApp.Application.Routes.Commands.UpdateRoute;

public record UpdateRouteCommand(
    int Id,
    int CourierId,
    int? VehicleId,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime) : IRequest;

public class UpdateRouteCommandHandler : IRequestHandler<UpdateRouteCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateRouteCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(UpdateRouteCommand request, CancellationToken cancellationToken)
    {
        var route = await _context.FindAsync<Route>(request.Id, cancellationToken);
        Guard.Against.NotFound(request.Id, route);

        route.CourierId = request.CourierId;
        route.VehicleId = request.VehicleId;
        route.Date = request.Date;
        route.StartTime = request.StartTime;
        route.EndTime = request.EndTime;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
