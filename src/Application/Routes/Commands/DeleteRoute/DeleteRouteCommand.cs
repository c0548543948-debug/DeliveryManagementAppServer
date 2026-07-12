using DeliveryManagementApp.Application.Common.Interfaces;

namespace DeliveryManagementApp.Application.Routes.Commands.DeleteRoute;

public record DeleteRouteCommand(int Id) : IRequest;

public class DeleteRouteCommandHandler : IRequestHandler<DeleteRouteCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteRouteCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(DeleteRouteCommand request, CancellationToken cancellationToken)
    {
        var route = await _context.FindAsync<DeliveryManagementApp.Domain.Entities.Route>(request.Id, cancellationToken);
        Guard.Against.NotFound(request.Id, route);

        _context.Remove<DeliveryManagementApp.Domain.Entities.Route>(route);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
