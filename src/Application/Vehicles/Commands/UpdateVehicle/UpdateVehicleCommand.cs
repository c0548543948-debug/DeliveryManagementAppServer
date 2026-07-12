using DeliveryManagementApp.Application.Common.Interfaces;
using DeliveryManagementApp.Domain.Enums;

namespace DeliveryManagementApp.Application.Vehicles.Commands.UpdateVehicle;

public record UpdateVehicleCommand(int Id, VehicleType Type, decimal CapacityVolume, decimal CapacityWeight, string LicensePlate) : IRequest;

public class UpdateVehicleCommandHandler : IRequestHandler<UpdateVehicleCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateVehicleCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(UpdateVehicleCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await _context.FindAsync<Vehicle>(request.Id, cancellationToken);
        Guard.Against.NotFound(request.Id, vehicle);

        vehicle.Type = request.Type;
        vehicle.CapacityVolume = request.CapacityVolume;
        vehicle.CapacityWeight = request.CapacityWeight;
        vehicle.LicensePlate = request.LicensePlate;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
