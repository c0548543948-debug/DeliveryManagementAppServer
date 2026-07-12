using FluentValidation;

namespace DeliveryManagementApp.Application.Vehicles.Commands.CreateVehicle;

public class CreateVehicleCommandValidator : AbstractValidator<CreateVehicleCommand>
{
    public CreateVehicleCommandValidator()
    {
        RuleFor(x => x.LicensePlate).NotEmpty().MaximumLength(20);
        RuleFor(x => x.CapacityVolume).GreaterThan(0);
        RuleFor(x => x.CapacityWeight).GreaterThan(0);
    }
}
