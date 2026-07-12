using DeliveryManagementApp.Application.Common.Interfaces;

namespace DeliveryManagementApp.Application.Couriers.Commands.DeleteCourier;

public record DeleteCourierCommand(int Id) : IRequest;

public class DeleteCourierCommandHandler : IRequestHandler<DeleteCourierCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteCourierCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(DeleteCourierCommand request, CancellationToken cancellationToken)
    {
        var courier = await _context.FindAsync<DeliveryManagementApp.Domain.Entities.Courier>(request.Id, cancellationToken);
        Guard.Against.NotFound(request.Id, courier);

        _context.Remove(courier);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
