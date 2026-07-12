using DeliveryManagementApp.Application.Common.Interfaces;
using DeliveryManagementApp.Domain.Entities;
using DeliveryManagementApp.Domain.Enums;

namespace DeliveryManagementApp.Application.Vehicles.Commands.CreateVehicle;

public record CreateVehicleCommand(VehicleType Type, decimal CapacityVolume, decimal CapacityWeight, string LicensePlate) : IRequest<int>;

public class CreateVehicleCommandHandler : IRequestHandler<CreateVehicleCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateVehicleCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<int> Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
    {
        var vehicle = new Vehicle
        {
            Type = request.Type,
            CapacityVolume = request.CapacityVolume,
            CapacityWeight = request.CapacityWeight,
            LicensePlate = request.LicensePlate
        };
        _context.Add(vehicle);
        await _context.SaveChangesAsync(cancellationToken);
        return vehicle.Id;
    }
}
