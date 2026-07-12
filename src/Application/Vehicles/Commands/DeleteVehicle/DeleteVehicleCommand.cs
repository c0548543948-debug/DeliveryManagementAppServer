using DeliveryManagementApp.Application.Common.Interfaces;

namespace DeliveryManagementApp.Application.Vehicles.Commands.DeleteVehicle;

public record DeleteVehicleCommand(int Id) : IRequest;

public class DeleteVehicleCommandHandler : IRequestHandler<DeleteVehicleCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteVehicleCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(DeleteVehicleCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await _context.FindAsync<Domain.Entities.Vehicle>(request.Id, cancellationToken);
        Guard.Against.NotFound(request.Id, vehicle);

        _context.Remove(vehicle);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
