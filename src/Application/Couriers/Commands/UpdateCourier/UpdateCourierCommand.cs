using DeliveryManagementApp.Application.Common.Interfaces;

namespace DeliveryManagementApp.Application.Couriers.Commands.UpdateCourier;

public record UpdateCourierCommand(int Id, string ApplicationUserId) : IRequest;

public class UpdateCourierCommandHandler : IRequestHandler<UpdateCourierCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateCourierCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(UpdateCourierCommand request, CancellationToken cancellationToken)
    {
        var courier = await _context.FindAsync<Courier>(request.Id, cancellationToken);
        Guard.Against.NotFound(request.Id, courier);

        courier.ApplicationUserId = request.ApplicationUserId;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
